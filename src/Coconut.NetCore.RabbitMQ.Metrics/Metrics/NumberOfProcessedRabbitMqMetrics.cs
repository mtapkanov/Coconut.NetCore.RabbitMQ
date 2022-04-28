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

        public void IncrementAcknowledgedMessagesCount(string exchange, string routing_key, int increment = 1)
        {
            _rabbitMqMessagesCountTotal.WithLabels($"{exchange}.{routing_key}", true).Inc(increment);
        }

        public void IncrementRejectedMessagesCount(string exchange, string routing_key, int increment = 1)
        {
            _rabbitMqMessagesCountTotal.WithLabels($"{exchange}.{routing_key}", false).Inc(increment);
        }
    }
}
