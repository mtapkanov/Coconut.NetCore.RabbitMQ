using Coconut.NetCore.RabbitMQ.Core.Events;
using Coconut.NetCore.RabbitMQ.Core.Handlers;
using Coconut.NetCore.RabbitMQ.Internal;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Coconut.NetCore.RabbitMQ.Tests.Internal;

public sealed class RabbitMqEventBusTests
{
    [Fact]
    public async Task FailingHandler_IsLoggedAndDoesNotStopNextHandlers()
    {
        var failing = Substitute.For<IRabbitMqEventHandler>();
        failing.HandleAsync(Arg.Any<IRabbitMqEvent>(), Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("boom"));
        var next = Substitute.For<IRabbitMqEventHandler>();
        var logger = new TestLogger();
        var bus = new RabbitMqEventBus([failing, next], new Logger<RabbitMqEventBus>(logger));
        var @event = new MessageAcknowledgedEvent("q", "ex", "rk", 1, false, new Dictionary<string, object?>());

        await bus.PublishAsync(@event, TestContext.Current.CancellationToken);

        await next.Received(1).HandleAsync(@event, Arg.Any<CancellationToken>());
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
    }
}
