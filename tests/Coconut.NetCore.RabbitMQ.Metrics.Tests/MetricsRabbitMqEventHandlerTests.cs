using Coconut.NetCore.RabbitMQ.Core.Events;
using Coconut.NetCore.RabbitMQ.Metrics.Metrics;

namespace Coconut.NetCore.RabbitMQ.Metrics.Tests;

public sealed class MetricsRabbitMqEventHandlerTests
{
    private static readonly Dictionary<string, object?> NoData = [];

    [Fact]
    public async Task AcknowledgedAndRejected_IncrementSeparateSeries()
    {
        var metrics = new NumberOfProcessedRabbitMqMetrics();
        var handler = new MetricsRabbitMqEventHandler(metrics);
        var ct = TestContext.Current.CancellationToken;

        await handler.HandleAsync(new MessageAcknowledgedEvent("q", "metrics-test-ex", "rk", 1, false, NoData), ct);
        await handler.HandleAsync(new MessageAcknowledgedEvent("q", "metrics-test-ex", "rk", 2, false, NoData), ct);
        await handler.HandleAsync(new MessageRejectedEvent("q", "metrics-test-ex", "rk", 3, false, NoData, Requeued: true, Exception: null), ct);

        // Same definition returns the already registered counter.
        var counter = StrictMetrics.Action(NumberOfProcessedRabbitMqMetrics.Name, "Number of processed RabbitMQ messages.", "RMQ messages", MetricValueType.Integer, MetricChangeTrackingPeriod.PerMinute);
        Assert.Equal(2, counter.WithLabels("metrics-test-ex.rk", success: true).Value);
        Assert.Equal(1, counter.WithLabels("metrics-test-ex.rk", success: false).Value);
    }
}
