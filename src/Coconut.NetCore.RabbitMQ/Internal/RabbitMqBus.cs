namespace Coconut.NetCore.RabbitMQ.Internal;

internal sealed class RabbitMqBus(PublisherRegistry registry) : IRabbitMqBus
{
    public ValueTask PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        var publishers = registry.Get<TMessage>();

        // Common case: one exchange per message type, no async state machine.
        return publishers.Length == 1
            ? publishers[0].PublishAsync(message, cancellationToken)
            : PublishToAllAsync(publishers, message, cancellationToken);
    }

    private static async ValueTask PublishToAllAsync<TMessage>(IRabbitMqPublisher<TMessage>[] publishers, TMessage message, CancellationToken cancellationToken)
    {
        foreach (var publisher in publishers)
            await publisher.PublishAsync(message, cancellationToken).ConfigureAwait(false);
    }
}
