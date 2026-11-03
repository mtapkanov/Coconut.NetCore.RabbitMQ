using Coconut.NetCore.RabbitMQ.Internal;
using Microsoft.Extensions.Time.Testing;
using RabbitMQ.Client;

namespace Coconut.NetCore.RabbitMQ.Tests.Internal;

public sealed class PublishPropertiesFactoryTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Defaults_PersistentWithTimeOrderedIdTimestampAndContentType()
    {
        var factory = new PublishPropertiesFactory<string>(persistent: true, "application/json", configure: null, new FakeTimeProvider(Now));

        var first = factory.Create("a");
        var second = factory.Create("b");

        Assert.Equal(DeliveryModes.Persistent, first.DeliveryMode);
        Assert.Equal("application/json", first.ContentType);
        Assert.Equal(Now.ToUnixTimeSeconds(), first.Timestamp.UnixTime);
        Assert.True(Guid.TryParseExact(first.MessageId, "N", out var id));
        Assert.Equal(7, id.Version);
        Assert.NotEqual(first.MessageId, second.MessageId);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void Transient_AndConfigureRunsAfterDefaults()
    {
        var factory = new PublishPropertiesFactory<string>(
            persistent: false,
            contentType: null,
            configure: static (message, properties) =>
            {
                properties.CorrelationId = message;
                properties.MessageId = "custom";
            },
            new FakeTimeProvider(Now));

        var properties = factory.Create("corr");

        Assert.Equal(DeliveryModes.Transient, properties.DeliveryMode);
        Assert.Null(properties.ContentType);
        Assert.Equal("corr", properties.CorrelationId);
        Assert.Equal("custom", properties.MessageId);
    }
}
