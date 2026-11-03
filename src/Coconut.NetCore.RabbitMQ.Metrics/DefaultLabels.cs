namespace Coconut.NetCore.RabbitMQ.Metrics;

/// <summary>
///     Static labels attached to every metric created through <see cref="StrictMetrics" />.
/// </summary>
public static class DefaultLabels
{
    private const string DisplayLabelName = "display";
    private const string ValueTypeLabelName = "value_type";
    private const string ChangeTrackingPeriodLabelName = "change_tracking_period";

    /// <summary>
    ///     Build static labels, skipping null values.
    /// </summary>
    public static Dictionary<string, string> GetStaticLabels(
        string? display = null,
        MetricValueType? valueType = null,
        MetricChangeTrackingPeriod? changeTrackingPeriod = null)
    {
        var labels = new Dictionary<string, string>(capacity: 3);

        if (display is not null)
            labels[DisplayLabelName] = display;

        if (valueType is not null)
            labels[ValueTypeLabelName] = valueType.ToString();

        if (changeTrackingPeriod is not null)
            labels[ChangeTrackingPeriodLabelName] = changeTrackingPeriod.ToString();

        return labels;
    }
}
