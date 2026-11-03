namespace Coconut.NetCore.RabbitMQ.Configuration.Settings;

/// <summary>
///     Exchange the bus publishes to.
/// </summary>
public sealed class RabbitMqExchangeSettings
{
    /// <summary>
    ///     Exchange name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Declaration parameters. Null means the exchange is expected to exist already.
    /// </summary>
    public RabbitMqExchangeDeclareSettings? Declare { get; init; }

    /// <summary>
    ///     If true (default), publishing waits for the broker confirm and
    ///     <see cref="IRabbitMqBus.PublishAsync{TMessage}" /> throws <c>RabbitMQ.Client.Exceptions.PublishException</c>
    ///     when the broker nacks the message or returns it as unroutable.
    ///     If false, publishing is fire-and-forget and unroutable messages are only logged.
    /// </summary>
    public bool PublisherConfirms { get; init; } = true;

    /// <summary>
    ///     If true (default), messages are published with delivery mode 2 (persistent): together with a durable queue
    ///     they survive a broker restart. Costs a disk write per message on classic queues.
    ///     Quorum and stream queues persist every message regardless of this flag.
    /// </summary>
    public bool PersistentMessages { get; init; } = true;
}

/// <summary>
///     Exchange declaration parameters.
/// </summary>
public sealed class RabbitMqExchangeDeclareSettings
{
    /// <summary>
    ///     Exchange type. Direct by default.
    /// </summary>
    public ExchangeType Type { get; init; } = ExchangeType.Direct;

    /// <summary>
    ///     If true, the exchange survives a broker restart.
    /// </summary>
    public bool Durable { get; init; }

    /// <summary>
    ///     If true, the exchange is deleted when the last queue is unbound from it.
    /// </summary>
    public bool AutoDelete { get; init; }

    /// <summary>
    ///     Optional arguments used by plugins and broker-specific features.
    /// </summary>
    public IDictionary<string, object?>? Arguments { get; init; }
}
