using Prometheus;

namespace Coconut.NetCore.RabbitMQ.Metrics
{
    /// <summary>
    ///     The metric which represent a counter with 'action' and 'result' labels.
    /// </summary>
    public class ActionMetric
    {
        /// <summary>
        ///     Creates the metric which represent a counter with 'action' and 'result' labels.
        /// </summary>
        /// <param name="name">Metric name</param>
        /// <param name="help">Metric description</param>
        /// <param name="display">Display name</param>
        /// <param name="valueType">Type of metric value</param>
        /// <param name="changeTrackingPeriod">Period of metric change</param>
        public ActionMetric(
            string name,
            string help,
            string display,
            MetricValueType valueType,
            MetricChangeTrackingPeriod changeTrackingPeriod)
        {
            Metric = Prometheus.Metrics.CreateCounter(name, help, new CounterConfiguration
            {
                LabelNames = new[] { "action", "result" },
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
        /// <param name="action">'action' label value</param>
        /// <param name="success">Was the action successful.</param>
        public Counter.Child WithLabels(string action, bool success)
        {
            return Metric.WithLabels(action, success ? "success" : "failure");
        }
    }
}
