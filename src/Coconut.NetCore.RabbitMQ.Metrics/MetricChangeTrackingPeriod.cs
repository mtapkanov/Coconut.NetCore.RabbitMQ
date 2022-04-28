namespace Coconut.NetCore.RabbitMQ.Metrics
{
    /// <summary>
    ///     Period of metric change
    /// </summary>
    public class MetricChangeTrackingPeriod
    {
        /// <summary>
        ///     Measure metric every second
        /// </summary>
        public static MetricChangeTrackingPeriod PerSecond { get; } = new MetricChangeTrackingPeriod("ps");

        /// <summary>
        ///     Measure metric every minute
        /// </summary>
        public static MetricChangeTrackingPeriod PerMinute { get; } = new MetricChangeTrackingPeriod("pm");

        private readonly string _value;

        private MetricChangeTrackingPeriod(string value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public override string ToString() => _value;
    }
}
