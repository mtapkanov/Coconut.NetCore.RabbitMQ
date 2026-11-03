using System.Collections.Frozen;

namespace Coconut.NetCore.RabbitMQ.Internal;

/// <summary>
///     Message type to publishers map. Mutable during startup, frozen once all connections are up.
/// </summary>
internal sealed class PublisherRegistry
{
    private readonly Lock _sync = new();
    private readonly Dictionary<Type, object> _pending = [];
    private FrozenDictionary<Type, object>? _frozen;

    public void Add<TMessage>(IRabbitMqPublisher<TMessage> publisher)
    {
        lock (_sync)
        {
            if (_frozen is not null)
                throw new InvalidOperationException("Publisher registry is already frozen.");

            // Value is always IRabbitMqPublisher<TMessage>[] for key typeof(TMessage).
            _pending[typeof(TMessage)] = _pending.TryGetValue(typeof(TMessage), out var existing)
                ? (IRabbitMqPublisher<TMessage>[])[.. (IRabbitMqPublisher<TMessage>[])existing, publisher]
                : new[] { publisher };
        }
    }

    public void Freeze()
    {
        lock (_sync)
            Volatile.Write(ref _frozen, _pending.ToFrozenDictionary());
    }

    public IRabbitMqPublisher<TMessage>[] Get<TMessage>()
    {
        var frozen = Volatile.Read(ref _frozen)
            ?? throw new InvalidOperationException(
                $"RabbitMQ bus is not started yet. Message of type {typeof(TMessage).FullName} cannot be published before the RabbitMQ hosted service has connected to the broker.");

        return frozen.TryGetValue(typeof(TMessage), out var publishers)
            ? (IRabbitMqPublisher<TMessage>[])publishers
            : throw new NotSupportedException($"Message type {typeof(TMessage).FullName} is not configured for publishing.");
    }
}
