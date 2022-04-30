using System.Collections.Generic;

namespace Coconut.NetCore.RabbitMQ.Configuration.Settings
{
    /// <summary>
    ///     RabbitMQ queue declaration configuration.
    /// </summary>
    public class RabbitMqExchangeDeclareSettings
    {
        /// <summary>
        ///     If true, the exchange will survive a broker restart. Default false.
        /// </summary>
        public bool Durable { get; set; }

        /// <summary>
        ///     Exchange type.
        /// </summary>
        public ExchangeType Type { get; set; }

        /// <summary>
        ///     If true, the exchange will deleted when last queue is unbound from it. Default false.
        /// </summary>
        public bool AutoDelete { get; set; }

        /// <summary>
        ///     Optional. Used by plugins and broker-specific features.
        /// </summary>
        public IDictionary<string, object> Arguments { get; set; }

    }
}
