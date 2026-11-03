namespace Coconut.NetCore.RabbitMQ.Core.Events;

/// <summary>
///     Outcome of a single message consumption.
/// </summary>
/// <remarks>
///     Immutable snapshot of the delivery. It deliberately does not carry the broker delivery object:
///     the message body is owned by the client library and is invalid once the consumer callback returns.
/// </remarks>
/// <param name="Queue">Queue the message was consumed from.</param>
/// <param name="Exchange">Exchange the message was originally published to.</param>
/// <param name="RoutingKey">Routing key the message was published with.</param>
/// <param name="DeliveryTag">Channel-scoped delivery tag.</param>
/// <param name="Redelivered">True if the broker has delivered this message before.</param>
/// <param name="Data">User data attached by the consumer via the consume context.</param>
public abstract record ConsumptionEvent(
    string Queue,
    string Exchange,
    string RoutingKey,
    ulong DeliveryTag,
    bool Redelivered,
    IReadOnlyDictionary<string, object?> Data) : IRabbitMqEvent;

/// <summary>
///     Message consumed successfully and acknowledged.
/// </summary>
public sealed record MessageAcknowledgedEvent(
    string Queue,
    string Exchange,
    string RoutingKey,
    ulong DeliveryTag,
    bool Redelivered,
    IReadOnlyDictionary<string, object?> Data)
    : ConsumptionEvent(Queue, Exchange, RoutingKey, DeliveryTag, Redelivered, Data);

/// <summary>
///     Message consumption failed and the message was negatively acknowledged.
///     <see cref="Requeued" /> is true if the message was returned to the queue, false if it was dead-lettered or dropped.
///     <see cref="Exception" /> is the exception thrown by deserialization or the consumer, null if the consumer marked the context failed.
/// </summary>
public sealed record MessageRejectedEvent(
    string Queue,
    string Exchange,
    string RoutingKey,
    ulong DeliveryTag,
    bool Redelivered,
    IReadOnlyDictionary<string, object?> Data,
    bool Requeued,
    Exception? Exception)
    : ConsumptionEvent(Queue, Exchange, RoutingKey, DeliveryTag, Redelivered, Data);
