using Prometheus;

namespace Coconut.NetCore.RabbitMQ.Metrics
{
    /// <summary>
    ///     The metric which represent a gauge with 'count' label.
    /// </summary>
    public class CountMetric
    {
        /// <summary>
        ///     Creates the metric which represent a gauge with 'count' label.
        /// </summary>
        /// <param name="name">Metric name</param>
        /// <param name="help">Metric description</param>
        /// <param name="display">Display name</param>
        /// <param name="valueType">Type of metric value</param>
        public CountMetric(
            string name,
            string help,
            string display,
            MetricValueType valueType)
        {
            Metric = Prometheus.Metrics.CreateGauge(name, help, new GaugeConfiguration
            {
                LabelNames = new[] { "count" },
                StaticLabels = DefaultLabels.GetStaticLabels(
                    display: display,
                    valueType: valueType)
            });
        }

        /// <summary>
        ///     Prometheus metric.
        /// </summary>
        public Gauge Metric { get; }

        /// <summary>
        ///     Returns the metric with filled labels.
        /// </summary>
        /// <param name="count">'count' label value</param>
        public Gauge.Child WithLabels(string count)
        {
            return Metric.WithLabels(count);
        }
    }
}
