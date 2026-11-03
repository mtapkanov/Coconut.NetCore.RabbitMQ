using System.Collections.ObjectModel;
using Coconut.NetCore.RabbitMQ.Configuration.Settings;
using Coconut.NetCore.RabbitMQ.Core.Events;
using Coconut.NetCore.RabbitMQ.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Coconut.NetCore.RabbitMQ.Internal;

internal interface IQueueConsumer : IAsyncDisposable
{
    Task StartAsync(IConnection connection, CancellationToken cancellationToken, CancellationToken stoppingToken);

    /// <summary>
    ///     Stop receiving new deliveries. In-flight messages keep running until the stopping token fires.
    /// </summary>
    Task CancelAsync(CancellationToken cancellationToken);
}

internal sealed class QueueConsumer<TMessage>(
    RabbitMqQueueSettings settings,
    IMessageDeserializer<TMessage> deserializer,
    Func<IServiceProvider, IMessageConsumer<TMessage>> consumerFactory,
    IServiceProvider services,
    RabbitMqEventBus eventBus,
    TimeProvider timeProvider,
    ILogger logger) : IQueueConsumer
{
    private IChannel? _channel;
    private string? _consumerTag;

    public async Task StartAsync(IConnection connection, CancellationToken cancellationToken, CancellationToken stoppingToken)
    {
        if (settings.PrefetchCount < settings.ConsumerDispatchConcurrency)
            Log.PrefetchBelowConcurrency(logger, settings.Name, settings.PrefetchCount, settings.ConsumerDispatchConcurrency);

        var channel = await connection.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: false,
                    publisherConfirmationTrackingEnabled: false,
                    consumerDispatchConcurrency: settings.ConsumerDispatchConcurrency),
                cancellationToken)
            .ConfigureAwait(false);
        _channel = channel;

        // Per-consumer limit (global: false): global QoS is not supported by quorum queues.
        await channel.BasicQosAsync(0, settings.PrefetchCount, global: false, cancellationToken).ConfigureAwait(false);
        await DeclareAsync(channel, cancellationToken).ConfigureAwait(false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, delivery) => HandleAsync(channel, delivery, stoppingToken);

        _consumerTag = await channel.BasicConsumeAsync(settings.Name, autoAck: false, consumer, cancellationToken).ConfigureAwait(false);
        Log.ConsumerStarted(logger, settings.Name, settings.PrefetchCount, settings.ConsumerDispatchConcurrency);
    }

    public async Task CancelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true } channel && _consumerTag is { } tag)
            await channel.BasicCancelAsync(tag, noWait: false, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is null)
            return;

        if (_channel.IsOpen)
            await _channel.CloseAsync().ConfigureAwait(false);

        await _channel.DisposeAsync().ConfigureAwait(false);
    }

    internal async Task HandleAsync(IChannel channel, BasicDeliverEventArgs delivery, CancellationToken stoppingToken)
    {
        bool failed;
        bool? requeueOverride = null;
        Exception? error = null;
        Dictionary<string, object?>? data = null;

        try
        {
            var message = deserializer.Deserialize(delivery.Body.Span);
            var context = new ConsumeContext<TMessage>(message, settings.Name, delivery);

            // The scope must outlive ConsumeAsync: scoped dependencies are disposed together with it.
            var scope = services.CreateAsyncScope();
            await using (scope.ConfigureAwait(false))
            {
                await consumerFactory(scope.ServiceProvider).ConsumeAsync(context, stoppingToken).ConfigureAwait(false);
            }

            failed = context.Failed;
            requeueOverride = context.Requeue;
            data = context.Data;
        }
        catch (OperationCanceledException exception) when (stoppingToken.IsCancellationRequested)
        {
            // Interrupted by shutdown: never dead-letter a message just because the host is stopping.
            failed = true;
            requeueOverride = true;
            error = exception;
        }
        catch (Exception exception)
        {
            failed = true;
            error = exception;
        }

        IReadOnlyDictionary<string, object?> eventData = data is { Count: > 0 } ? data : ReadOnlyDictionary<string, object?>.Empty;

        if (!failed)
        {
            if (!await TrySettleAsync(channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, CancellationToken.None), delivery).ConfigureAwait(false))
                return;

            Log.MessageAcknowledged(logger, settings.Name, delivery.DeliveryTag);
            await eventBus.PublishAsync(
                    new MessageAcknowledgedEvent(settings.Name, delivery.Exchange, delivery.RoutingKey, delivery.DeliveryTag, delivery.Redelivered, eventData),
                    stoppingToken)
                .ConfigureAwait(false);
            return;
        }

        var requeue = requeueOverride ?? settings.RequeueOnFailure;

        if (!await TrySettleAsync(channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue, CancellationToken.None), delivery).ConfigureAwait(false))
            return;

        Log.MessageRejected(logger, error, settings.Name, delivery.DeliveryTag, delivery.Exchange, delivery.RoutingKey, requeue);
        await eventBus.PublishAsync(
                new MessageRejectedEvent(settings.Name, delivery.Exchange, delivery.RoutingKey, delivery.DeliveryTag, delivery.Redelivered, eventData, requeue, error),
                stoppingToken)
            .ConfigureAwait(false);

        if (requeue && settings.FailDelay > TimeSpan.Zero && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(settings.FailDelay, timeProvider, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Host is stopping, nothing to wait for.
            }
        }
    }

    // Ack/nack on a closed channel throws. The broker requeues unacknowledged deliveries of a closed channel by itself,
    // so the only thing to do is log it instead of failing the dispatcher.
    private async ValueTask<bool> TrySettleAsync(ValueTask settle, BasicDeliverEventArgs delivery)
    {
        try
        {
            await settle.ConfigureAwait(false);
            return true;
        }
        catch (Exception exception)
        {
            Log.SettleFailed(logger, exception, settings.Name, delivery.DeliveryTag);
            return false;
        }
    }

    private async Task DeclareAsync(IChannel channel, CancellationToken cancellationToken)
    {
        if (settings.Declare is not { } declare)
            return;

        await channel.QueueDeclareAsync(settings.Name, declare.Durable, declare.Exclusive, declare.AutoDelete, declare.Arguments, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (declare.Bindings is not { Count: > 0 } bindings)
        {
            Log.QueueWithoutBindings(logger, settings.Name);
            return;
        }

        foreach (var binding in bindings)
            await channel.QueueBindAsync(settings.Name, binding.Exchange, binding.RoutingKey, binding.Arguments, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
    }
}
