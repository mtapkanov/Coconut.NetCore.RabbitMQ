using System.Diagnostics.CodeAnalysis;
using Coconut.NetCore.RabbitMQ.Configuration.Settings;
using Coconut.NetCore.RabbitMQ.Internal;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Coconut.NetCore.RabbitMQ.Configuration.Builders;

/// <summary>
///     Configures one broker connection: its exchanges (publishing) and queues (consuming).
/// </summary>
public sealed class RabbitMqOptionsBuilder
{
    private readonly IServiceCollection _services;
    private readonly Uri _uri;
    private readonly List<RabbitMqExchangeBuilder> _exchanges = [];
    private readonly List<IQueueBuilder> _queues = [];
    private Action<ConnectionFactory>? _configureFactory;

    internal RabbitMqOptionsBuilder(IServiceCollection services, Uri uri)
    {
        _services = services;
        _uri = uri;
    }

    /// <summary>
    ///     Adjust the underlying <see cref="ConnectionFactory" /> (heartbeat, TLS, recovery interval, client name...).
    ///     Called after <see cref="ConnectionFactory.Uri" /> is set.
    /// </summary>
    public RabbitMqOptionsBuilder ConfigureConnectionFactory(Action<ConnectionFactory> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        _configureFactory += configure;
        return this;
    }

    /// <summary>
    ///     Add an exchange to publish to.
    /// </summary>
    public RabbitMqExchangeBuilder AddExchange(RabbitMqExchangeSettings exchangeSettings)
    {
        ArgumentNullException.ThrowIfNull(exchangeSettings);

        var builder = new RabbitMqExchangeBuilder(_services, exchangeSettings);
        _exchanges.Add(builder);
        return builder;
    }

    /// <summary>
    ///     Add a queue to consume <typeparamref name="TMessage" /> from.
    /// </summary>
    public RabbitMqQueueBuilder<TMessage> AddQueue<TMessage>(RabbitMqQueueSettings queueSettings)
    {
        ArgumentNullException.ThrowIfNull(queueSettings);

        var builder = new RabbitMqQueueBuilder<TMessage>(_services, queueSettings);
        _queues.Add(builder);
        return builder;
    }

    internal ConnectionDefinition Build() =>
        new(_uri, _configureFactory, [.. _exchanges.Select(x => x.Build())], [.. _queues.Select(x => x.Build())]);
}

/// <summary>
///     Configures which messages are published to an exchange.
/// </summary>
public sealed class RabbitMqExchangeBuilder
{
    private readonly IServiceCollection _services;
    private readonly RabbitMqExchangeSettings _settings;
    private readonly List<PublisherDefinition> _publishers = [];

    internal RabbitMqExchangeBuilder(IServiceCollection services, RabbitMqExchangeSettings settings)
    {
        _services = services;
        _settings = settings;
    }

    /// <summary>
    ///     Publish <typeparamref name="TMessage" /> to this exchange.
    ///     The serializer is registered as a singleton unless already registered.
    /// </summary>
    /// <param name="routingKey">Routing key selector. Empty routing key if null.</param>
    /// <param name="configureProperties">
    ///     Adjust message properties after the defaults are set (<c>message-id</c> = UUIDv7, <c>timestamp</c>, <c>delivery-mode</c>, <c>content-type</c>):
    ///     correlation id, headers, priority, expiration. Called on every publish, keep it allocation-light.
    /// </param>
    public RabbitMqExchangeBuilder AcceptMessages<
        TMessage,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSerializer>(
        Func<TMessage, string>? routingKey = null,
        Action<TMessage, BasicProperties>? configureProperties = null)
        where TSerializer : class, Processing.IMessageSerializer<TMessage>
    {
        _services.TryAddSingletonSelf<TSerializer>();
        _publishers.Add(new PublisherDefinition<TMessage, TSerializer>(routingKey ?? (static _ => string.Empty), configureProperties));
        return this;
    }

    internal ExchangeDefinition Build()
    {
        if (string.IsNullOrWhiteSpace(_settings.Name))
            throw new InvalidOperationException("Exchange name must be set.");

        if (_publishers.Count == 0)
            throw new InvalidOperationException($"Exchange '{_settings.Name}': at least one message type must be configured with {nameof(AcceptMessages)}.");

        return new ExchangeDefinition(_settings, [.. _publishers]);
    }
}

/// <summary>
///     Configures how messages from a queue are deserialized and consumed.
/// </summary>
public sealed class RabbitMqQueueBuilder<TMessage> : IQueueBuilder
{
    private readonly IServiceCollection _services;
    private readonly RabbitMqQueueSettings _settings;
    private Func<IServiceProvider, Processing.IMessageDeserializer<TMessage>>? _deserializer;
    private Func<IServiceProvider, Processing.IMessageConsumer<TMessage>>? _consumer;

    internal RabbitMqQueueBuilder(IServiceCollection services, RabbitMqQueueSettings settings)
    {
        _services = services;
        _settings = settings;
    }

    /// <summary>
    ///     Deserializer for queue messages. Registered as a singleton unless already registered.
    /// </summary>
    public RabbitMqQueueBuilder<TMessage> UseDeserializer<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDeserializer>()
        where TDeserializer : class, Processing.IMessageDeserializer<TMessage>
    {
        _services.TryAddSingletonSelf<TDeserializer>();
        _deserializer = static sp => sp.GetRequiredService<TDeserializer>();
        return this;
    }

    /// <summary>
    ///     Consumer for queue messages. Resolved from a new DI scope per message.
    /// </summary>
    /// <param name="lifetime">Consumer lifetime. Scoped by default.</param>
    public RabbitMqQueueBuilder<TMessage> UseConsumer<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TConsumer>(
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TConsumer : class, Processing.IMessageConsumer<TMessage>
    {
        _services.TryAddSelf<TConsumer>(lifetime);
        _consumer = static sp => sp.GetRequiredService<TConsumer>();
        return this;
    }

    QueueDefinition IQueueBuilder.Build()
    {
        if (string.IsNullOrWhiteSpace(_settings.Name))
            throw new InvalidOperationException("Queue name must be set.");

        if (_settings.PrefetchCount == 0)
            throw new InvalidOperationException($"Queue '{_settings.Name}': {nameof(RabbitMqQueueSettings.PrefetchCount)} must be greater than zero.");

        if (_settings.ConsumerDispatchConcurrency == 0)
            throw new InvalidOperationException($"Queue '{_settings.Name}': {nameof(RabbitMqQueueSettings.ConsumerDispatchConcurrency)} must be greater than zero.");

        return new QueueDefinition<TMessage>(
            _settings,
            _deserializer ?? throw new InvalidOperationException($"Queue '{_settings.Name}': deserializer must be configured with {nameof(UseDeserializer)}."),
            _consumer ?? throw new InvalidOperationException($"Queue '{_settings.Name}': consumer must be configured with {nameof(UseConsumer)}."));
    }
}

internal interface IQueueBuilder
{
    QueueDefinition Build();
}
