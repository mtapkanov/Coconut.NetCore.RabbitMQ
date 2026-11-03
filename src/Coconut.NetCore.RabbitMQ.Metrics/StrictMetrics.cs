using Prometheus;

namespace Coconut.NetCore.RabbitMQ.Metrics;

/// <summary>
///     Factory for metrics with the standard static label set (<see cref="DefaultLabels" />).
///     Metrics are registered in <see cref="Prometheus.Metrics.DefaultRegistry" />; creating a metric with an existing name returns the registered one.
/// </summary>
public static class StrictMetrics
{
    /// <summary>
    ///     Counter of actions labeled by action name and result (success/failure).
    /// </summary>
    public static ActionMetric Action(string name, string help, string display, MetricValueType valueType, MetricChangeTrackingPeriod changeTrackingPeriod) =>
        new(name, help, display, valueType, changeTrackingPeriod);

    /// <summary>
    ///     Counter labeled by a <c>total</c> label.
    /// </summary>
    public static TotalMetric Total(string name, string help, string display, MetricValueType valueType, MetricChangeTrackingPeriod changeTrackingPeriod) =>
        new(name, help, display, valueType, changeTrackingPeriod);

    /// <summary>
    ///     Gauge labeled by a <c>count</c> label.
    /// </summary>
    public static CountMetric Count(string name, string help, string display, MetricValueType valueType) =>
        new(name, help, display, valueType);
}

/// <summary>
///     Counter of actions labeled by action name and result.
/// </summary>
public sealed class ActionMetric(string name, string help, string display, MetricValueType valueType, MetricChangeTrackingPeriod changeTrackingPeriod)
{
    /// <summary>
    ///     Underlying counter.
    /// </summary>
    public Counter Metric { get; } = Prometheus.Metrics
        .WithLabels(DefaultLabels.GetStaticLabels(display, valueType, changeTrackingPeriod))
        .CreateCounter(name, help, labelNames: ["action", "result"]);

    /// <summary>
    ///     Series for an action and its result.
    /// </summary>
    public Counter.Child WithLabels(string action, bool success) => Metric.WithLabels(action, success ? "success" : "failure");
}

/// <summary>
///     Counter labeled by a <c>total</c> label.
/// </summary>
public sealed class TotalMetric(string name, string help, string display, MetricValueType valueType, MetricChangeTrackingPeriod changeTrackingPeriod)
{
    /// <summary>
    ///     Underlying counter.
    /// </summary>
    public Counter Metric { get; } = Prometheus.Metrics
        .WithLabels(DefaultLabels.GetStaticLabels(display, valueType, changeTrackingPeriod))
        .CreateCounter(name, help, labelNames: ["total"]);

    /// <summary>
    ///     Series for a label value.
    /// </summary>
    public Counter.Child WithLabels(string total) => Metric.WithLabels(total);
}

/// <summary>
///     Gauge labeled by a <c>count</c> label.
/// </summary>
public sealed class CountMetric(string name, string help, string display, MetricValueType valueType)
{
    /// <summary>
    ///     Underlying gauge.
    /// </summary>
    public Gauge Metric { get; } = Prometheus.Metrics
        .WithLabels(DefaultLabels.GetStaticLabels(display, valueType))
        .CreateGauge(name, help, labelNames: ["count"]);

    /// <summary>
    ///     Series for a label value.
    /// </summary>
    public Gauge.Child WithLabels(string count) => Metric.WithLabels(count);
}
