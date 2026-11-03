using Coconut.NetCore.RabbitMQ.Internal;
using Coconut.NetCore.RabbitMQ.Tests.Fakes;

namespace Coconut.NetCore.RabbitMQ.Tests.Internal;

public sealed class RabbitMqBusTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public async Task PublishAsync_FansOutToEveryPublisherOfTheType(int publisherCount)
    {
        var registry = new PublisherRegistry();
        var publishers = Enumerable.Range(0, publisherCount).Select(_ => new FakePublisher<string>()).ToArray();
        foreach (var publisher in publishers)
            registry.Add(publisher);
        registry.Freeze();

        await new RabbitMqBus(registry).PublishAsync("hello", TestContext.Current.CancellationToken);

        Assert.All(publishers, publisher => Assert.Equal(["hello"], publisher.Published));
    }
}
