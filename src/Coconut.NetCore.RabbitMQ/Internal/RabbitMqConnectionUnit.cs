using Coconut.NetCore.RabbitMQ.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Coconut.NetCore.RabbitMQ.Internal;

/// <summary>
///     One broker connection with its publisher channels (one per exchange) and consumer channels (one per queue).
/// </summary>
internal sealed class RabbitMqConnectionUnit(
    ConnectionDefinition definition,
    IServiceProvider services,
    PublisherRegistry registry,
    RabbitMqEventBus eventBus,
    TimeProvider timeProvider,
    ILoggerFactory loggerFactory) : IAsyncDisposable
{
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30);

    private readonly ILogger _logger = loggerFactory.CreateLogger("Coconut.NetCore.RabbitMQ.Connection");
    private readonly List<IChannel> _publisherChannels = [];
    private readonly List<IQueueConsumer> _consumers = [];
    private IConnection? _connection;

    public async Task StartAsync(CancellationToken cancellationToken, CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            Uri = definition.Uri,
            ClientProvidedName = AppDomain.CurrentDomain.FriendlyName
        };
        definition.ConfigureFactory?.Invoke(factory);

        var connection = await ConnectAsync(factory, cancellationToken).ConfigureAwait(false);
        _connection = connection;

        var publisherLogger = loggerFactory.CreateLogger("Coconut.NetCore.RabbitMQ.Publisher");
        foreach (var exchange in definition.Exchanges)
        {
            var confirms = exchange.Settings.PublisherConfirms;
            var channel = await connection.CreateChannelAsync(new CreateChannelOptions(confirms, confirms), cancellationToken).ConfigureAwait(false);
            _publisherChannels.Add(channel);

            // With confirm tracking the client turns basic.return into PublishException for the caller.
            if (!confirms)
                channel.BasicReturnAsync += (_, args) => LogReturnAsync(publisherLogger, args);

            if (exchange.Settings.Declare is { } declare)
                await channel.ExchangeDeclareAsync(exchange.Settings.Name, declare.Type.ToAmqp(), declare.Durable, declare.AutoDelete, declare.Arguments, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            foreach (var publisher in exchange.Publishers)
                publisher.Register(registry, channel, exchange.Settings, services, publisherLogger);
        }

        var consumerLogger = loggerFactory.CreateLogger("Coconut.NetCore.RabbitMQ.Consumer");
        foreach (var queue in definition.Queues)
        {
            var consumer = queue.CreateConsumer(services, eventBus, timeProvider, consumerLogger);
            _consumers.Add(consumer);
            await consumer.StartAsync(connection, cancellationToken, stoppingToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Stop new deliveries on every queue of this connection.
    /// </summary>
    public async Task CancelConsumersAsync(CancellationToken cancellationToken)
    {
        foreach (var consumer in _consumers)
        {
            try
            {
                await consumer.CancelAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Log.ShutdownStepFailed(_logger, exception, definition.Uri.Host);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var consumer in _consumers)
            await DisposeQuietlyAsync(consumer).ConfigureAwait(false);

        foreach (var channel in _publisherChannels)
            await DisposeQuietlyAsync(channel).ConfigureAwait(false);

        if (_connection is not null)
            await DisposeQuietlyAsync(_connection).ConfigureAwait(false);
    }

    private async Task<IConnection> ConnectAsync(ConnectionFactory factory, CancellationToken cancellationToken)
    {
        // Only the initial connect is retried here; afterwards the client's automatic recovery takes over
        // (connection, channels, QoS, consumers and declared topology).
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                var connection = await factory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
                Log.Connected(_logger, factory.Endpoint.HostName, factory.Endpoint.Port, factory.VirtualHost);
                return connection;
            }
            catch (BrokerUnreachableException exception) when (!cancellationToken.IsCancellationRequested)
            {
                var delay = TimeSpan.FromSeconds(Math.Min(attempt, MaxRetryDelay.TotalSeconds));
                Log.ConnectFailed(_logger, exception, factory.Endpoint.HostName, factory.Endpoint.Port, attempt, delay);
                await Task.Delay(delay, timeProvider, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static Task LogReturnAsync(ILogger logger, BasicReturnEventArgs args)
    {
        Log.MessageReturned(logger, args.Exchange, args.RoutingKey, args.ReplyCode, args.ReplyText);
        return Task.CompletedTask;
    }

    private async ValueTask DisposeQuietlyAsync(IAsyncDisposable disposable)
    {
        try
        {
            switch (disposable)
            {
                case IChannel { IsOpen: true } channel:
                    await channel.CloseAsync().ConfigureAwait(false);
                    break;
                case IConnection { IsOpen: true } connection:
                    await connection.CloseAsync().ConfigureAwait(false);
                    break;
            }

            await disposable.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            Log.ShutdownStepFailed(_logger, exception, definition.Uri.Host);
        }
    }
}
