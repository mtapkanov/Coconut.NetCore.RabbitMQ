using System.Collections.Generic;

namespace Coconut.NetCore.RabbitMQ.Metrics
{
    /// <summary>
    ///     Default metrics labels
    /// </summary>
    public static class DefaultLabels
    {
        private const string DisplayLabelName = "display";
        private const string ValueTypeLabelName = "value_type";
        private const string ChangeTrackingPeriodLabelName = "change_tracking_period";

        /// <summary>
        ///     Get default static labels
        /// </summary>
        /// <param name="display">Display name</param>
        /// <param name="valueType">Type of metric value</param>
        /// <param name="changeTrackingPeriod">Period of metric change</param>
        public static Dictionary<string, string> GetStaticLabels(string display = null, MetricValueType valueType = null, MetricChangeTrackingPeriod changeTrackingPeriod = null)
        {
            var labels = new Dictionary<string, string>();

            if (display != null)
                labels.Add(DisplayLabelName, display);

            if (valueType != null)
                labels.Add(ValueTypeLabelName, valueType.ToString());

            if (changeTrackingPeriod != null)
                labels.Add(ChangeTrackingPeriodLabelName, changeTrackingPeriod.ToString());

            return labels;
        }
    }
}
