namespace Coconut.NetCore.RabbitMQ.Metrics
{
    /// <summary>
    ///     Type of metric value
    /// </summary>
    public class MetricValueType
    {
        /// <summary>
        ///     Integer number
        /// </summary>
        public static MetricValueType Integer { get; } = new MetricValueType("integer");

        /// <summary>
        ///     Floating point number
        /// </summary>
        public static MetricValueType Float { get; } = new MetricValueType("float");

        /// <summary>
        ///     Text
        /// </summary>
        public static MetricValueType Text { get; } = new MetricValueType("text");

        private readonly string _value;

        private MetricValueType(string value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public override string ToString() => _value;
    }
}
