using System;
using System.Collections.Generic;

namespace Coconut.NetCore.RabbitMQ.Configuration
{
    /// <summary>
    ///     RabbitMQ exchange options.
    /// </summary>
    public class RabbitMqExchangeOptions
    {
        /// <summary>
        ///     RabbitMQ exchange configuration.
        /// </summary>
        public RabbitMqExchangeSettings ExchangeSettings { get; }

        /// <summary>
        ///     The publish options of messages that will be distributed through the exchange.
        /// </summary>
        public List<RabbitMqPublishOptions> AcceptedPublishOptions { get; }

        /// <summary>
        ///     Creates RabbitMQ exchange options.
        /// </summary>
        /// <param name="exchangeSettings">RabbitMQ exchange configuration.</param>
        /// <param name="acceptedPublishOptions">The types of messages that will be distributed through the exchange.</param>
        public RabbitMqExchangeOptions(RabbitMqExchangeSettings exchangeSettings, List<RabbitMqPublishOptions> acceptedPublishOptions)
        {
            ExchangeSettings = exchangeSettings ?? throw new ArgumentNullException(nameof(exchangeSettings));
            AcceptedPublishOptions = acceptedPublishOptions ?? throw new ArgumentNullException(nameof(acceptedPublishOptions));
        }
    }
}
