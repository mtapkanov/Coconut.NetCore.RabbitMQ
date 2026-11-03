namespace Coconut.NetCore.RabbitMQ;

/// <summary>
///     Publishes messages to every exchange configured to accept the message type.
///     Thread-safe.
/// </summary>
public interface IRabbitMqBus
{
    /// <summary>
    ///     Publish a message. Message type is matched exactly: base types and interfaces are not considered.
    /// </summary>
    /// <exception cref="InvalidOperationException">The bus has not connected to the broker yet.</exception>
    /// <exception cref="NotSupportedException">No exchange accepts <typeparamref name="TMessage" />.</exception>
    /// <exception cref="global::RabbitMQ.Client.Exceptions.PublishException">
    ///     Publisher confirms are enabled and the broker nacked the message or returned it as unroutable.
    /// </exception>
    ValueTask PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default);
}
