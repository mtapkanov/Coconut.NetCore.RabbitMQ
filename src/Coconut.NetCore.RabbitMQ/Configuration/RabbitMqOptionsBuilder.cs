using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Coconut.NetCore.RabbitMQ.Configuration
{
    /// <summary>
    ///     RabbitMQ unit options builder.
    /// </summary>
    public class RabbitMqOptionsBuilder : IBuilder<RabbitMqOptions>
    {
        private readonly List<IBuilder<RabbitMqExchangeOptions>> _exchangeBuilders = new();
        private readonly List<IBuilder<RabbitMqQueueOptions>> _queueBuilders = new();
        private readonly IServiceCollection _services;
        private readonly Uri _uri;

        /// <summary>
        ///     Creates RabbitMQ unit options builder.
        /// </summary>
        /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
        /// <param name="uri">RabbitMQ AMQP URI containing credentials.</param>
        public RabbitMqOptionsBuilder(IServiceCollection services, Uri uri)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        /// <summary>
        ///     Defines RabbitMQ exchange.
        /// </summary>
        /// <param name="exchangeSettings">RabbitMQ exchange configuration.</param>
        public RabbitMqExchangeOptionsBuilder AddExchange(RabbitMqExchangeSettings exchangeSettings)
        {
            var exchangeBuilder = new RabbitMqExchangeOptionsBuilder(_services, exchangeSettings);
            _exchangeBuilders.Add(exchangeBuilder);

            return exchangeBuilder;
        }

        /// <summary>
        ///     Defines RabbitMQ queue.
        /// </summary>
        /// <param name="queueSettings">RabbitMQ queue configuration.</param>
        /// <typeparam name="TMessage">The type representing messages in the queue.</typeparam>
        public RabbitMqQueueOptionsBuilder<TMessage> AddQueue<TMessage>(RabbitMqQueueSettings queueSettings)
        {
            var queueBuilder = new RabbitMqQueueOptionsBuilder<TMessage>(_services, queueSettings);
            _queueBuilders.Add(queueBuilder);

            return queueBuilder;
        }

        /// <inheritdoc />
        public RabbitMqOptions Build() =>
            new(
                _uri,
                _exchangeBuilders.Select(x => x.Build()).ToArray(),
                _queueBuilders.Select(x => x.Build()).ToArray()
            );
    }
}
