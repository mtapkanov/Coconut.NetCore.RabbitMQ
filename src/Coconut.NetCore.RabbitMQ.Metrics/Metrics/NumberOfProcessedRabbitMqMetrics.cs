namespace Coconut.NetCore.RabbitMQ.Metrics.Metrics
{
    internal class NumberOfProcessedRabbitMqMetrics
    {
        private readonly ActionMetric _rabbitMqMessagesCountTotal = StrictMetrics.Action(
            "app_rmq_messages_count_total",
            "Number of processed RabbitMQ messages.",
            "RMQ messages",
            MetricValueType.Integer,
            MetricChangeTrackingPeriod.PerMinute);

        public void IncrementAcknowledgedMessagesCount(string exchange, string routingKey, int increment = 1)
        {
            _rabbitMqMessagesCountTotal.WithLabels($"{exchange}.{routingKey}", true).Inc(increment);
        }

        public void IncrementRejectedMessagesCount(string exchange, string routingKey, int increment = 1)
        {
            _rabbitMqMessagesCountTotal.WithLabels($"{exchange}.{routingKey}", false).Inc(increment);
        }
    }
}
