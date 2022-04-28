using Prometheus;

namespace Coconut.NetCore.RabbitMQ.Metrics
{
    /// <summary>
    ///     The metric which represent a counter with 'total' label.
    /// </summary>
    public class TotalMetric
    {
        /// <summary>
        ///     Creates the metric which represent a counter with 'total' label.
        /// </summary>
        /// <param name="name">Metric name</param>
        /// <param name="help">Metric description</param>
        /// <param name="display">Display name</param>
        /// <param name="valueType">Type of metric value</param>
        /// <param name="changeTrackingPeriod">Period of metric change</param>
        public TotalMetric(
            string name,
            string help,
            string display,
            MetricValueType valueType,
            MetricChangeTrackingPeriod changeTrackingPeriod)
        {
            Metric = Prometheus.Metrics.CreateCounter(name, help, new CounterConfiguration
            {
                LabelNames = new[] { "total" },
                StaticLabels = DefaultLabels.GetStaticLabels(
                    display: display,
                    valueType: valueType,
                    changeTrackingPeriod: changeTrackingPeriod)
            });
        }

        /// <summary>
        ///     Prometheus metric.
        /// </summary>
        public Counter Metric { get; }

        /// <summary>
        ///     Returns the metric with filled labels.
        /// </summary>
        /// <param name="total">'total' label value</param>
        public Counter.Child WithLabels(string total)
        {
            return Metric.WithLabels(total);
        }
    }
}
