using Coconut.NetCore.RabbitMQ.Processing;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Coconut.NetCore.RabbitMQ.Internal;

internal interface IRabbitMqPublisher<in TMessage>
{
    ValueTask PublishAsync(TMessage message, CancellationToken cancellationToken);
}

/// <remarks>
///     <see cref="IChannel.BasicPublishAsync{TProperties}(string, string, bool, TProperties, ReadOnlyMemory{byte}, CancellationToken)" />
///     is safe for concurrent use in RabbitMQ.Client 7: each command is serialized into a single buffer before it is written,
///     and with confirm tracking the channel semaphore covers only sequence number assignment and the write, not the wait for the confirm.
///     So no extra locking here, and concurrent publishes are pipelined.
/// </remarks>
internal sealed class RabbitMqPublisher<TMessage>(
    IChannel channel,
    string exchange,
    IMessageSerializer<TMessage> serializer,
    Func<TMessage, string> routingKey,
    PublishPropertiesFactory<TMessage> properties,
    ILogger logger) : IRabbitMqPublisher<TMessage>
{
    public async ValueTask PublishAsync(TMessage message, CancellationToken cancellationToken)
    {
        var key = routingKey(message);
        var body = serializer.Serialize(message);

        try
        {
            await channel.BasicPublishAsync(exchange, key, mandatory: true, properties.Create(message), body, cancellationToken).ConfigureAwait(false);
            Log.MessagePublished(logger, exchange, key);
        }
        catch (PublishException exception)
        {
            Log.MessageNotConfirmed(logger, exception, exchange, key, exception.IsReturn);
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Log.PublishFailed(logger, exception, exchange, key);
            throw;
        }
    }
}
