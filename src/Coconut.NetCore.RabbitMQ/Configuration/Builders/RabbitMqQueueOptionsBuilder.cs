using System;
using Coconut.NetCore.RabbitMQ.Configuration.Options;
using Coconut.NetCore.RabbitMQ.Configuration.Settings;
using Coconut.NetCore.RabbitMQ.Processing;
using Coconut.NetCore.RabbitMQ.Processing.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Coconut.NetCore.RabbitMQ.Configuration.Builders
{
    /// <summary>
    ///     RabbitMQ queue options builder.
    /// </summary>
    /// <typeparam name="TMessage">The type representing messages in the queue.</typeparam>
    public class RabbitMqQueueOptionsBuilder<TMessage> : IBuilder<RabbitMqQueueOptions>
    {
        private readonly IServiceCollection _services;
        private readonly RabbitMqQueueSettings _queueSettings;
        private Type _deserializerType;
        private Type _consumerType;

        /// <summary>
        ///     Creates RabbitMQ queue options builder.
        /// </summary>
        /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
        /// <param name="queueSettings">RabbitMQ queue configuration.</param>
        public RabbitMqQueueOptionsBuilder(IServiceCollection services, RabbitMqQueueSettings queueSettings)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _queueSettings = queueSettings ?? throw new ArgumentNullException(nameof(queueSettings));
        }

        /// <summary>
        ///     Registers message deserializer implementation.
        /// </summary>
        /// <typeparam name="TMessageDeserializer">Type of message deserializer.</typeparam>
        public RabbitMqQueueOptionsBuilder<TMessage> UseDeserializer<TMessageDeserializer>()
            where TMessageDeserializer : IMessageDeserializer<TMessage>
        {
            _deserializerType = typeof(TMessageDeserializer);
            _services.TryAddSingleton(_deserializerType);

            return this;
        }

        /// <summary>
        ///     Registers message consumer implementation.
        /// </summary>
        /// <typeparam name="TMessageConsumer">Type of message consumer.</typeparam>
        /// <param name="serviceLifetime">Specifies the lifetime of a consumer. Scoped by default.</param>
        public RabbitMqQueueOptionsBuilder<TMessage> UseConsumer<TMessageConsumer>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where TMessageConsumer : MessageConsumerBase<TMessage>
        {
            _consumerType = typeof(TMessageConsumer);
            _services.TryAdd(ServiceDescriptor.Describe(_consumerType, _consumerType, serviceLifetime));

            return this;
        }

        /// <inheritdoc />
        public RabbitMqQueueOptions Build()
        {
            if (_deserializerType is null)
                throw new NotSupportedException($"Message deserializer must be defined in {nameof(RabbitMqQueueOptionsBuilder<TMessage>)}");

            if (_consumerType is null)
                throw new NotSupportedException($"Message consumer must be defined in {nameof(RabbitMqQueueOptionsBuilder<TMessage>)}");

            return new RabbitMqQueueOptions(_queueSettings, typeof(TMessage), _deserializerType, _consumerType);
        }
    }
}
