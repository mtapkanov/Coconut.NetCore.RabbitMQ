namespace Coconut.NetCore.RabbitMQ.Core.Events;

/// <summary>
///     Marker for events raised by the RabbitMQ bus and delivered to <see cref="Handlers.IRabbitMqEventHandler" />.
/// </summary>
public interface IRabbitMqEvent;
