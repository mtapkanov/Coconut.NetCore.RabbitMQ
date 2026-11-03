namespace Coconut.NetCore.RabbitMQ.Metrics.Metrics;

internal sealed class NumberOfProcessedRabbitMqMetrics
{
    public const string Name = "app_rmq_messages_count_total";

    private readonly ActionMetric _messagesCountTotal = StrictMetrics.Action(
        Name,
        "Number of processed RabbitMQ messages.",
        "RMQ messages",
        MetricValueType.Integer,
        MetricChangeTrackingPeriod.PerMinute);

    public void IncrementAcknowledged(string exchange, string routingKey) =>
        _messagesCountTotal.WithLabels($"{exchange}.{routingKey}", success: true).Inc();

    public void IncrementRejected(string exchange, string routingKey) =>
        _messagesCountTotal.WithLabels($"{exchange}.{routingKey}", success: false).Inc();
}
