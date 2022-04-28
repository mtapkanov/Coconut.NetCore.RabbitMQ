namespace Coconut.NetCore.RabbitMQ.Metrics
{
    /// <summary>
    ///     Creates metrics that meet the requirements from the documentation:
    ///     https://confluence.fbsvc.bz/confluence/display/BPO/Service+Metrics
    /// </summary>
    public static class StrictMetrics
    {
        /// <summary>
        ///     Creates the metric which represent a counter with 'action' and 'result' labels.
        /// </summary>
        /// <param name="name">Metric name</param>
        /// <param name="help">Metric description</param>
        /// <param name="display">Display name</param>
        /// <param name="valueType">Type of metric value</param>
        /// <param name="changeTrackingPeriod">Period of metric change</param>
        public static ActionMetric Action(
            string name,
            string help,
            string display,
            MetricValueType valueType,
            MetricChangeTrackingPeriod changeTrackingPeriod) => new(name, help, display, valueType, changeTrackingPeriod);

        /// <summary>
        ///     Creates the metric which represent a counter with 'total' label.
        /// </summary>
        /// <param name="name">Metric name</param>
        /// <param name="help">Metric description</param>
        /// <param name="display">Display name</param>
        /// <param name="valueType">Type of metric value</param>
        /// <param name="changeTrackingPeriod">Period of metric change</param>
        public static TotalMetric Total(
            string name,
            string help,
            string display,
            MetricValueType valueType,
            MetricChangeTrackingPeriod changeTrackingPeriod) => new(name, help, display, valueType, changeTrackingPeriod);

        /// <summary>
        ///     Creates the metric which represent a gauge with 'count' label.
        /// </summary>
        /// <param name="name">Metric name</param>
        /// <param name="help">Metric description</param>
        /// <param name="display">Display name</param>
        /// <param name="valueType">Type of metric value</param>
        public static CountMetric Count(
            string name,
            string help,
            string display,
            MetricValueType valueType) => new(name, help, display, valueType);
    }
}
