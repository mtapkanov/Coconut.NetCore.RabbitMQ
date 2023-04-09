using System;
using System.Collections.Generic;
using System.Linq;
using Coconut.NetCore.RabbitMQ.Configuration.Options;
using Coconut.NetCore.RabbitMQ.Configuration.Settings;
using Coconut.NetCore.RabbitMQ.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Coconut.NetCore.RabbitMQ.Configuration.Builders
{
    /// <summary>
    ///     RabbitMQ exchange options builder.
    /// </summary>
    public class RabbitMqExchangeOptionsBuilder : IBuilder<RabbitMqExchangeOptions>
    {
        private readonly IServiceCollection _services;
        private readonly RabbitMqExchangeSettings _exchangeSettings;

        private readonly List<RabbitMqPublishOptions> _publishOptions = new();

        /// <summary>
        ///     Creates RabbitMQ exchange options builder.
        /// </summary>
        /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
        /// <param name="exchangeSettings">RabbitMQ exchange configuration.</param>
        public RabbitMqExchangeOptionsBuilder(IServiceCollection services, RabbitMqExchangeSettings exchangeSettings)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _exchangeSettings = exchangeSettings ?? throw new ArgumentNullException(nameof(exchangeSettings));
        }

        /// <summary>
        ///     Adds the type of messages that will be published to this exchange.
        /// </summary>
        /// <typeparam name="TMessage">The type of message that will be distributed through the exchange.</typeparam>
        /// <typeparam name="TMessageSerializer">Type of message serializer.</typeparam>
        public RabbitMqExchangeOptionsBuilder AcceptMessages<TMessage, TMessageSerializer>(Func<TMessage, string> routingKeyProvider = null)
            where TMessageSerializer : MessageSerializerBase<TMessage>
        {
            Func<object, string> getRoutingKey = routingKeyProvider is null
                ? (object message) => string.Empty
                : (object message) => routingKeyProvider((TMessage)message);

            _publishOptions.Add(new RabbitMqPublishOptions(typeof(TMessage), typeof(TMessageSerializer), getRoutingKey));

            _services.TryAddSingleton(typeof(TMessageSerializer));

            return this;
        }

        /// <inheritdoc />
        public RabbitMqExchangeOptions Build()
        {
            if (!_publishOptions.Any()) 
                throw new NotSupportedException($"Message types must be defined in {nameof(RabbitMqExchangeOptionsBuilder)}");

            return new RabbitMqExchangeOptions(_exchangeSettings, _publishOptions);
        }
    }
}
