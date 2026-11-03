using Coconut.NetCore.RabbitMQ.Configuration.Settings;
using Coconut.NetCore.RabbitMQ.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Coconut.NetCore.RabbitMQ.Internal;

// Definitions are built once in AddRabbitMq and carry the message type as a generic parameter,
// so the runtime path never needs reflection or casts from object.

internal sealed record ConnectionDefinition(
    Uri Uri,
    Action<ConnectionFactory>? ConfigureFactory,
    IReadOnlyList<ExchangeDefinition> Exchanges,
    IReadOnlyList<QueueDefinition> Queues);

internal sealed record ExchangeDefinition(RabbitMqExchangeSettings Settings, IReadOnlyList<PublisherDefinition> Publishers);

internal abstract class PublisherDefinition
{
    public abstract void Register(PublisherRegistry registry, IChannel channel, RabbitMqExchangeSettings exchange, IServiceProvider services, ILogger logger);
}

internal sealed class PublisherDefinition<TMessage, TSerializer>(
    Func<TMessage, string> routingKey,
    Action<TMessage, BasicProperties>? configureProperties) : PublisherDefinition
    where TSerializer : class, IMessageSerializer<TMessage>
{
    public override void Register(PublisherRegistry registry, IChannel channel, RabbitMqExchangeSettings exchange, IServiceProvider services, ILogger logger)
    {
        IMessageSerializer<TMessage> serializer = services.GetRequiredService<TSerializer>();
        var properties = new PublishPropertiesFactory<TMessage>(
            exchange.PersistentMessages, serializer.ContentType, configureProperties, services.GetRequiredService<TimeProvider>());

        registry.Add(new RabbitMqPublisher<TMessage>(channel, exchange.Name, serializer, routingKey, properties, logger));
    }
}

internal abstract class QueueDefinition(RabbitMqQueueSettings settings)
{
    public RabbitMqQueueSettings Settings { get; } = settings;

    public abstract IQueueConsumer CreateConsumer(IServiceProvider services, RabbitMqEventBus eventBus, TimeProvider timeProvider, ILogger logger);
}

internal sealed class QueueDefinition<TMessage>(
    RabbitMqQueueSettings settings,
    Func<IServiceProvider, IMessageDeserializer<TMessage>> deserializer,
    Func<IServiceProvider, IMessageConsumer<TMessage>> consumer) : QueueDefinition(settings)
{
    public override IQueueConsumer CreateConsumer(IServiceProvider services, RabbitMqEventBus eventBus, TimeProvider timeProvider, ILogger logger) =>
        new QueueConsumer<TMessage>(Settings, deserializer(services), consumer, services, eventBus, timeProvider, logger);
}
