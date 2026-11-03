using Coconut.NetCore.RabbitMQ.Internal;
using Coconut.NetCore.RabbitMQ.Tests.Fakes;

namespace Coconut.NetCore.RabbitMQ.Tests.Internal;

public sealed class PublisherRegistryTests
{
    [Fact]
    public void Get_BeforeFreeze_ThrowsInvalidOperation()
    {
        var registry = new PublisherRegistry();
        registry.Add(new FakePublisher<string>());

        Assert.Throws<InvalidOperationException>(() => registry.Get<string>());
    }

    [Fact]
    public void Get_UnknownType_ThrowsNotSupported()
    {
        var registry = new PublisherRegistry();
        registry.Freeze();

        Assert.Throws<NotSupportedException>(() => registry.Get<string>());
    }

    [Fact]
    public void Get_ReturnsAllPublishersInRegistrationOrder_ExactTypeOnly()
    {
        var registry = new PublisherRegistry();
        var first = new FakePublisher<string>();
        var second = new FakePublisher<string>();
        registry.Add(first);
        registry.Add(second);
        registry.Freeze();

        Assert.Equal([first, second], registry.Get<string>());
        Assert.Throws<NotSupportedException>(() => registry.Get<object>());
    }

    [Fact]
    public void Add_AfterFreeze_Throws()
    {
        var registry = new PublisherRegistry();
        registry.Freeze();

        Assert.Throws<InvalidOperationException>(() => registry.Add(new FakePublisher<string>()));
    }
}
