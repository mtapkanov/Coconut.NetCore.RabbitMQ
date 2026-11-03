using RabbitMQ.Client.Events;

namespace Coconut.NetCore.RabbitMQ.Processing;

/// <summary>
///     Incoming message with its delivery details and the consumer's verdict.
/// </summary>
/// <param name="message">Deserialized message.</param>
/// <param name="queue">Queue the message was consumed from.</param>
/// <param name="delivery">Raw delivery. Its body is owned by the client library and is invalid after the consumer returns.</param>
public sealed class ConsumeContext<TMessage>(TMessage message, string queue, BasicDeliverEventArgs delivery)
{
    /// <summary>
    ///     Deserialized message.
    /// </summary>
    public TMessage Message { get; } = message;

    /// <summary>
    ///     Queue the message was consumed from.
    /// </summary>
    public string Queue { get; } = queue;

    /// <summary>
    ///     Raw delivery: exchange, routing key, delivery tag, redelivered flag, properties.
    ///     Do not keep a reference to <see cref="BasicDeliverEventArgs.Body" /> after the consumer returns.
    /// </summary>
    public BasicDeliverEventArgs Delivery { get; } = delivery;

    /// <summary>
    ///     If true, the message is negatively acknowledged instead of acknowledged.
    /// </summary>
    public bool Failed { get; set; }

    /// <summary>
    ///     Per-message override of <see cref="Configuration.Settings.RabbitMqQueueSettings.RequeueOnFailure" />.
    ///     Applies only when consumption failed. Null means use the queue setting.
    ///     Set false for permanent failures (validation, unknown format) to avoid an endless redelivery loop.
    /// </summary>
    public bool? Requeue { get; set; }

    /// <summary>
    ///     User data passed to <see cref="Core.Events.ConsumptionEvent.Data" />.
    /// </summary>
    public Dictionary<string, object?> Data { get; } = [];

    /// <summary>
    ///     Mark consumption failed.
    /// </summary>
    /// <param name="requeue">Requeue override, see <see cref="Requeue" />.</param>
    public void Fail(bool? requeue = null)
    {
        Failed = true;
        Requeue = requeue;
    }
}
