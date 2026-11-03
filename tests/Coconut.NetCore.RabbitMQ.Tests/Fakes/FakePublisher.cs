using System.Collections.Concurrent;
using Coconut.NetCore.RabbitMQ.Internal;

namespace Coconut.NetCore.RabbitMQ.Tests.Fakes;

/// <summary>Hand-written fake for the internal publisher contract: records every published message.</summary>
internal sealed class FakePublisher<TMessage> : IRabbitMqPublisher<TMessage>
{
    private readonly ConcurrentQueue<TMessage> _published = new();

    public IReadOnlyCollection<TMessage> Published => _published;

    public ValueTask PublishAsync(TMessage message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _published.Enqueue(message);
        return ValueTask.CompletedTask;
    }
}
