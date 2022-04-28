using System.Collections.Generic;

namespace Coconut.NetCore.RabbitMQ.Configuration
{
    /// <summary>
    ///     RabbitMQ queue declaration configuration.
    /// </summary>
    public class RabbitMqQueueDeclareSettings
    {
        /// <summary>
        ///     If true, the queue will survive a broker restart. Default false.
        /// </summary>
        public bool Durable { get; set; }

        /// <summary>
        ///     If true, the queue will be used by only one connection and the queue will be deleted when that connection closes. Default false.
        /// </summary>
        public bool Exclusive { get; set; }

        /// <summary>
        ///     If true, the queue will delete itself after at least one consumer has connected, and then all consumers have disconnected. Default false.
        /// </summary>
        public bool AutoDelete { get; set; }

        /// <summary>
        ///     Optional. Used by plugins and broker-specific features.
        /// </summary>
        public IDictionary<string, object> Arguments { get; set; }

        /// <summary>
        ///     Queue bindings configuration.
        /// </summary>
        public IList<RabbitMqQueueBindingSettings> Bindings { get; set; }


    }
}
