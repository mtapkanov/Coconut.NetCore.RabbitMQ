using System.Text;
using Coconut.NetCore.RabbitMQ.Internal;
using Coconut.NetCore.RabbitMQ.Processing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Coconut.NetCore.RabbitMQ.Tests.Internal;

public sealed class RabbitMqPublisherTests
{
    private sealed class Utf8Serializer : IMessageSerializer<string>
    {
        public ReadOnlyMemory<byte> Serialize(string message) => Encoding.UTF8.GetBytes(message);
    }

    private static (IChannel Channel, RabbitMqPublisher<string> Publisher, TestLogger Logger) Create()
    {
        var channel = Substitute.For<IChannel>();
        var logger = new TestLogger();
        var properties = new PublishPropertiesFactory<string>(persistent: true, "text/plain", configure: null, TimeProvider.System);
        var publisher = new RabbitMqPublisher<string>(channel, "exchange", new Utf8Serializer(), message => $"rk.{message}", properties, logger);
        return (channel, publisher, logger);
    }

    [Fact]
    public async Task PublishAsync_SendsMandatoryMessageWithRoutingKeyAndBody()
    {
        var (channel, publisher, _) = Create();
        ReadOnlyMemory<byte> sentBody = default;
        await channel.BasicPublishAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<BasicProperties>(), Arg.Do<ReadOnlyMemory<byte>>(body => sentBody = body), Arg.Any<CancellationToken>());

        await publisher.PublishAsync("hello", TestContext.Current.CancellationToken);

        await channel.Received(1).BasicPublishAsync(
            "exchange",
            "rk.hello",
            true,
            Arg.Is<BasicProperties>(p => p.Persistent && p.ContentType == "text/plain" && p.MessageId != null),
            Arg.Any<ReadOnlyMemory<byte>>(),
            Arg.Any<CancellationToken>());
        Assert.Equal("hello", Encoding.UTF8.GetString(sentBody.Span));
    }

    [Fact]
    public async Task PublishAsync_BrokerReturn_PropagatesPublishExceptionAndLogs()
    {
        var (channel, publisher, logger) = Create();
        channel
            .BasicPublishAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<BasicProperties>(), Arg.Any<ReadOnlyMemory<byte>>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException(new PublishException(publishSequenceNumber: 7, isReturn: true)));

        var exception = await Assert.ThrowsAsync<PublishException>(() => publisher.PublishAsync("hello", TestContext.Current.CancellationToken).AsTask());

        Assert.True(exception.IsReturn);
        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Contains("rk.hello", entry.Message);
    }

    [Fact]
    public async Task PublishAsync_Cancelled_IsNotLoggedAsError()
    {
        var (channel, publisher, logger) = Create();
        channel
            .BasicPublishAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<BasicProperties>(), Arg.Any<ReadOnlyMemory<byte>>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException(new OperationCanceledException()));

        await Assert.ThrowsAsync<OperationCanceledException>(() => publisher.PublishAsync("hello", TestContext.Current.CancellationToken).AsTask());

        Assert.Empty(logger.Entries);
    }
}
