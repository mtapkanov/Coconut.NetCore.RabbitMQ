using Coconut.NetCore.RabbitMQ.Core.Events;
using Coconut.NetCore.RabbitMQ.Core.Extensions;
using Coconut.NetCore.RabbitMQ.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Coconut.NetCore.RabbitMQ.Core.Tests;

public sealed class AddRabbitMqEventHandlerTests
{
    private sealed class Handler : IRabbitMqEventHandler
    {
        public ValueTask HandleAsync(IRabbitMqEvent @event, CancellationToken cancellationToken) => ValueTask.CompletedTask;
    }

    [Fact]
    public void SameHandlerTwice_IsRegisteredOnce()
    {
        var provider = new ServiceCollection()
            .AddRabbitMqEventHandler<Handler>()
            .AddRabbitMqEventHandler<Handler>()
            .BuildServiceProvider();

        Assert.Single(provider.GetServices<IRabbitMqEventHandler>());
    }

    [Fact]
    public void Events_HaveValueEquality()
    {
        var data = new Dictionary<string, object?>();

        Assert.Equal(
            new MessageAcknowledgedEvent("q", "ex", "rk", 1, false, data),
            new MessageAcknowledgedEvent("q", "ex", "rk", 1, false, data));
    }
}
