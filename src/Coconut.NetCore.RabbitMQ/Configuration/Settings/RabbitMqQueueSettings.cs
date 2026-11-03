namespace Coconut.NetCore.RabbitMQ.Configuration.Settings;

/// <summary>
///     Queue the bus consumes from.
/// </summary>
public sealed class RabbitMqQueueSettings
{
    /// <summary>
    ///     Queue name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Maximum number of unacknowledged messages delivered to the consumer. One by default.
    ///     Should be at least <see cref="ConsumerDispatchConcurrency" />, otherwise extra dispatch slots stay idle.
    /// </summary>
    public ushort PrefetchCount { get; init; } = 1;

    /// <summary>
    ///     Number of messages processed in parallel. One by default, which preserves delivery order.
    ///     Values above one give up ordering and require thread-safe consumers and their dependencies.
    /// </summary>
    public ushort ConsumerDispatchConcurrency { get; init; } = 1;

    /// <summary>
    ///     Pause after a requeued failure before the next message is processed.
    ///     Applies to the dispatch slot that failed, not to the requeued message itself.
    /// </summary>
    public TimeSpan FailDelay { get; init; }

    /// <summary>
    ///     If true (default), a failed message is returned to the queue and redelivered.
    ///     If false, it is rejected without requeue: the broker routes it to the dead-letter exchange configured via
    ///     queue argument <c>x-dead-letter-exchange</c> (see <see cref="RabbitMqQueueDeclareSettings.Arguments" />), or drops it.
    ///     Can be overridden per message with <see cref="Processing.ConsumeContext{TMessage}.Requeue" />.
    ///     Messages interrupted by host shutdown are always requeued.
    /// </summary>
    public bool RequeueOnFailure { get; init; } = true;

    /// <summary>
    ///     Declaration parameters. Null means the queue and its bindings are expected to exist already.
    /// </summary>
    public RabbitMqQueueDeclareSettings? Declare { get; init; }
}

/// <summary>
///     Queue declaration parameters.
/// </summary>
public sealed class RabbitMqQueueDeclareSettings
{
    /// <summary>
    ///     If true, the queue survives a broker restart.
    /// </summary>
    public bool Durable { get; init; }

    /// <summary>
    ///     If true, the queue is used by only one connection and deleted when that connection closes.
    /// </summary>
    public bool Exclusive { get; init; }

    /// <summary>
    ///     If true, the queue is deleted when the last consumer unsubscribes.
    /// </summary>
    public bool AutoDelete { get; init; }

    /// <summary>
    ///     Optional arguments: <c>x-queue-type</c>, <c>x-dead-letter-exchange</c>, <c>x-message-ttl</c>, etc.
    /// </summary>
    public IDictionary<string, object?>? Arguments { get; init; }

    /// <summary>
    ///     Bindings to declare. Without bindings the queue receives nothing except via the default exchange.
    /// </summary>
    public IReadOnlyList<RabbitMqQueueBindingSettings>? Bindings { get; init; }
}

/// <summary>
///     Queue to exchange binding.
/// </summary>
public sealed class RabbitMqQueueBindingSettings
{
    /// <summary>
    ///     Source exchange name.
    /// </summary>
    public required string Exchange { get; init; }

    /// <summary>
    ///     Binding key. Empty by default.
    /// </summary>
    public string RoutingKey { get; init; } = string.Empty;

    /// <summary>
    ///     Optional binding arguments (e.g. header matching rules).
    /// </summary>
    public IDictionary<string, object?>? Arguments { get; init; }
}
