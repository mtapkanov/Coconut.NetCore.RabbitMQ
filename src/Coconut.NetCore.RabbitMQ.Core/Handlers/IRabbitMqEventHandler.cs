using Coconut.NetCore.RabbitMQ.Core.Events;

namespace Coconut.NetCore.RabbitMQ.Core.Handlers;

/// <summary>
///     Receives bus events. Handlers are invoked sequentially on the consumer path after ack/nack,
///     so they add latency to every message: keep them cheap and non-blocking.
///     Exceptions are logged and swallowed by the bus.
/// </summary>
public interface IRabbitMqEventHandler
{
    /// <summary>
    ///     Handle a bus event.
    /// </summary>
    ValueTask HandleAsync(IRabbitMqEvent @event, CancellationToken cancellationToken);
}
