using RabbitMQ.Client;

namespace Coconut.NetCore.RabbitMQ.Internal;

/// <summary>
///     Builds AMQP properties for every outgoing message. A new instance per message:
///     the client may add headers (tracing) to the properties it is given.
/// </summary>
internal sealed class PublishPropertiesFactory<TMessage>(
    bool persistent,
    string? contentType,
    Action<TMessage, BasicProperties>? configure,
    TimeProvider timeProvider)
{
    public BasicProperties Create(TMessage message)
    {
        var now = timeProvider.GetUtcNow();

        var properties = new BasicProperties
        {
            // Time-ordered id: sortable in logs and index-friendly if a consumer stores it for deduplication.
            MessageId = Guid.CreateVersion7(now).ToString("N"),
            Timestamp = new AmqpTimestamp(now.ToUnixTimeSeconds()),
            DeliveryMode = persistent ? DeliveryModes.Persistent : DeliveryModes.Transient,
            ContentType = contentType
        };

        configure?.Invoke(message, properties);
        return properties;
    }
}
