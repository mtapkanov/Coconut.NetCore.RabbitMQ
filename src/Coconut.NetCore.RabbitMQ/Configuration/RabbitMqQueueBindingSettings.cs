using System.Collections.Generic;

namespace Coconut.NetCore.RabbitMQ.Configuration
{
    /// <summary>
    ///     RabbitMQ queue binding configuration.
    /// </summary>
    public class RabbitMqQueueBindingSettings
    {
        /// <summary>
        ///     Exchange name.
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        ///     Routing key name.
        /// </summary>
        public string RoutingKey { get; set; }

        /// <summary>
        ///     Optional. Used by plugins and broker-specific features.
        /// </summary>
        public IDictionary<string, object> Arguments { get; set; }

    }
}
