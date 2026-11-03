namespace Coconut.NetCore.RabbitMQ.Processing;

/// <summary>
///     Converts an outgoing message to its wire format. Registered as a singleton: must be thread-safe.
/// </summary>
public interface IMessageSerializer<in TMessage>
{
    /// <summary>
    ///     Serialize a message.
    /// </summary>
    ReadOnlyMemory<byte> Serialize(TMessage message);

    /// <summary>
    ///     MIME type written to the <c>content-type</c> message property, e.g. <c>application/json</c>. Null to omit.
    /// </summary>
    string? ContentType => null;
}

/// <summary>
///     Converts an incoming message body to a message. Registered as a singleton: must be thread-safe.
/// </summary>
public interface IMessageDeserializer<out TMessage>
{
    /// <summary>
    ///     Deserialize a message body. The span is only valid for the duration of the call.
    ///     An exception is treated as a consumption failure.
    /// </summary>
    TMessage Deserialize(ReadOnlySpan<byte> body);
}

/// <summary>
///     Handles messages from a queue. Resolved from a fresh DI scope for every message.
///     The message is acknowledged when <see cref="ConsumeAsync" /> completes and the context is not marked failed.
///     It is negatively acknowledged when the context is marked failed or the method throws.
/// </summary>
public interface IMessageConsumer<TMessage>
{
    /// <summary>
    ///     Consume a message.
    /// </summary>
    /// <param name="context">The message and its delivery details.</param>
    /// <param name="cancellationToken">Signalled on host shutdown.</param>
    Task ConsumeAsync(ConsumeContext<TMessage> context, CancellationToken cancellationToken);
}
