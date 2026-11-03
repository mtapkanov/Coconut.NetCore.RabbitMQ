namespace Coconut.NetCore.RabbitMQ.Metrics;

/// <summary>
///     Value type exported as the <c>value_type</c> static label.
/// </summary>
public sealed class MetricValueType
{
    private readonly string _value;

    private MetricValueType(string value) => _value = value;

    /// <summary>Integer value.</summary>
    public static MetricValueType Integer { get; } = new("integer");

    /// <summary>Floating point value.</summary>
    public static MetricValueType Float { get; } = new("float");

    /// <summary>Text value.</summary>
    public static MetricValueType Text { get; } = new("text");

    /// <inheritdoc />
    public override string ToString() => _value;
}

/// <summary>
///     Rate period exported as the <c>change_tracking_period</c> static label.
/// </summary>
public sealed class MetricChangeTrackingPeriod
{
    private readonly string _value;

    private MetricChangeTrackingPeriod(string value) => _value = value;

    /// <summary>Per second.</summary>
    public static MetricChangeTrackingPeriod PerSecond { get; } = new("ps");

    /// <summary>Per minute.</summary>
    public static MetricChangeTrackingPeriod PerMinute { get; } = new("pm");

    /// <inheritdoc />
    public override string ToString() => _value;
}
