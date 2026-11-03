using System.Text;
using Coconut.NetCore.RabbitMQ.Configuration.Settings;
using Coconut.NetCore.RabbitMQ.Core.Events;
using Coconut.NetCore.RabbitMQ.Core.Handlers;
using Coconut.NetCore.RabbitMQ.Internal;
using Coconut.NetCore.RabbitMQ.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Coconut.NetCore.RabbitMQ.Tests.Internal;

public sealed class QueueConsumerTests
{
    public enum Outcome { Success, Failed, Throws, Cancelled }

    private const ulong DeliveryTag = 42;

    private sealed class ScopedDependency : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }

    private sealed class Behaviour
    {
        public Outcome Outcome { get; init; }
        public bool? Requeue { get; init; }
        public bool DependencyDisposedDuringConsume { get; set; }
        public ScopedDependency? Dependency { get; set; }
    }

    private sealed class Utf8Deserializer : IMessageDeserializer<string>
    {
        public string Deserialize(ReadOnlySpan<byte> body) =>
            body.Length == 0 ? throw new FormatException("empty") : Encoding.UTF8.GetString(body);
    }

    private sealed class TestConsumer(ScopedDependency dependency, Behaviour behaviour, CancellationTokenSource stopping) : IMessageConsumer<string>
    {
        public async Task ConsumeAsync(ConsumeContext<string> context, CancellationToken cancellationToken)
        {
            await Task.Yield();
            behaviour.DependencyDisposedDuringConsume = dependency.Disposed;
            behaviour.Dependency = dependency;
            context.Data["seen"] = context.Message;

            switch (behaviour.Outcome)
            {
                case Outcome.Failed:
                    context.Fail(behaviour.Requeue);
                    break;
                case Outcome.Throws:
                    throw new InvalidOperationException("boom");
                case Outcome.Cancelled:
                    await stopping.CancelAsync();
                    cancellationToken.ThrowIfCancellationRequested();
                    break;
            }
        }
    }

    private sealed class RecordingHandler : IRabbitMqEventHandler
    {
        public List<IRabbitMqEvent> Events { get; } = [];

        public ValueTask HandleAsync(IRabbitMqEvent @event, CancellationToken cancellationToken)
        {
            Events.Add(@event);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class Harness
    {
        public required IChannel Channel { get; init; }
        public required QueueConsumer<string> Consumer { get; init; }
        public required Behaviour Behaviour { get; init; }
        public required RecordingHandler Events { get; init; }
        public required FakeTimeProvider Time { get; init; }
        public required CancellationTokenSource Stopping { get; init; }

        public Task DeliverAsync(string body = "hello") =>
            Consumer.HandleAsync(
                Channel,
                new BasicDeliverEventArgs("ctag", DeliveryTag, false, "exchange", "rk", new BasicProperties(), Encoding.UTF8.GetBytes(body)),
                Stopping.Token);
    }

    private static Harness Create(Outcome outcome, bool requeueOnFailure = true, bool? requeueOverride = null, TimeSpan failDelay = default)
    {
        var behaviour = new Behaviour { Outcome = outcome, Requeue = requeueOverride };
        var events = new RecordingHandler();
        var stopping = new CancellationTokenSource();
        var time = new FakeTimeProvider();

        var services = new ServiceCollection()
            .AddScoped<ScopedDependency>()
            .AddScoped<TestConsumer>()
            .AddSingleton(behaviour)
            .AddSingleton(stopping)
            .BuildServiceProvider();

        // Unconfigured ValueTask members of a substitute return a completed default(ValueTask).
        var channel = Substitute.For<IChannel>();

        var settings = new RabbitMqQueueSettings { Name = "queue", RequeueOnFailure = requeueOnFailure, FailDelay = failDelay };
        var logger = new TestLogger();
        var consumer = new QueueConsumer<string>(
            settings,
            new Utf8Deserializer(),
            static sp => sp.GetRequiredService<TestConsumer>(),
            services,
            new RabbitMqEventBus([events], new Logger<RabbitMqEventBus>(logger)),
            time,
            logger);

        return new Harness { Channel = channel, Consumer = consumer, Behaviour = behaviour, Events = events, Time = time, Stopping = stopping };
    }

    [Fact]
    public async Task ScopedConsumerDependencies_AreAliveDuringConsume_AndDisposedAfter()
    {
        var harness = Create(Outcome.Success);

        await harness.DeliverAsync();

        Assert.False(harness.Behaviour.DependencyDisposedDuringConsume);
        Assert.True(harness.Behaviour.Dependency!.Disposed);
    }

    [Fact]
    public async Task Success_AcksAndRaisesAcknowledgedEventWithData()
    {
        var harness = Create(Outcome.Success);

        await harness.DeliverAsync();

        await harness.Channel.Received(1).BasicAckAsync(DeliveryTag, false, Arg.Any<CancellationToken>());
        await harness.Channel.DidNotReceive().BasicNackAsync(Arg.Any<ulong>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        var @event = Assert.IsType<MessageAcknowledgedEvent>(Assert.Single(harness.Events.Events));
        Assert.Equal(("queue", "exchange", "rk", DeliveryTag), (@event.Queue, @event.Exchange, @event.RoutingKey, @event.DeliveryTag));
        Assert.Equal("hello", @event.Data["seen"]);
    }

    [Theory]
    [InlineData(Outcome.Failed, true, null, true)]
    [InlineData(Outcome.Failed, false, null, false)]
    [InlineData(Outcome.Failed, true, false, false)]
    [InlineData(Outcome.Failed, false, true, true)]
    [InlineData(Outcome.Throws, true, null, true)]
    [InlineData(Outcome.Throws, false, null, false)]
    [InlineData(Outcome.Cancelled, false, null, true)]
    public async Task Failure_NacksWithResolvedRequeue(Outcome outcome, bool requeueOnFailure, bool? requeueOverride, bool expectedRequeue)
    {
        var harness = Create(outcome, requeueOnFailure, requeueOverride);

        await harness.DeliverAsync();

        await harness.Channel.Received(1).BasicNackAsync(DeliveryTag, false, expectedRequeue, Arg.Any<CancellationToken>());
        await harness.Channel.DidNotReceive().BasicAckAsync(Arg.Any<ulong>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        var @event = Assert.IsType<MessageRejectedEvent>(Assert.Single(harness.Events.Events));
        Assert.Equal(expectedRequeue, @event.Requeued);
    }

    [Fact]
    public async Task DeserializationFailure_IsRejectedWithException()
    {
        var harness = Create(Outcome.Success, requeueOnFailure: false);

        await harness.DeliverAsync(body: "");

        await harness.Channel.Received(1).BasicNackAsync(DeliveryTag, false, false, Arg.Any<CancellationToken>());
        var @event = Assert.IsType<MessageRejectedEvent>(Assert.Single(harness.Events.Events));
        Assert.IsType<FormatException>(@event.Exception);
    }

    [Fact]
    public async Task FailDelay_AppliesOnlyToRequeuedFailures()
    {
        var delay = TimeSpan.FromSeconds(5);

        var deadLettered = Create(Outcome.Failed, requeueOnFailure: false, failDelay: delay);
        // Completes without advancing fake time: no delay for a dead-lettered message.
        await deadLettered.DeliverAsync().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        var requeued = Create(Outcome.Failed, requeueOnFailure: true, failDelay: delay);
        var handling = requeued.DeliverAsync();
        await WaitUntil(() => requeued.Events.Events.Count == 1);
        Assert.False(handling.IsCompleted);

        // The delay timer is created right after the event is raised; keep advancing until it has been scheduled and fired.
        await WaitUntil(() =>
        {
            requeued.Time.Advance(delay);
            return handling.IsCompleted;
        });
        await handling;
    }

    [Fact]
    public async Task AckOnClosedChannel_DoesNotThrowAndRaisesNoEvent()
    {
        var harness = Create(Outcome.Success);
        harness.Channel
            .BasicAckAsync(Arg.Any<ulong>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException(new InvalidOperationException("channel closed")));

        await harness.DeliverAsync();

        Assert.Empty(harness.Events.Events);
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!condition())
            await Task.Delay(10, timeout.Token);
    }
}
