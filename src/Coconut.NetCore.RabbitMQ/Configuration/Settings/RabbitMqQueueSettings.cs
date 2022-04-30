using System;

namespace Coconut.NetCore.RabbitMQ.Configuration.Settings
{
    /// <summary>
    ///     RabbitMQ queue configuration.
    /// </summary>

    public class RabbitMqQueueSettings
    {
        /// <summary>
        ///     Queue name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Limit the number of unacknowledged messages. One by default.
        /// </summary>
        public ushort PrefetchCount { get; set; } = 1;

        /// <summary>
        ///     Failed message consumption wait duration before retry.
        /// </summary>
        public TimeSpan FailDelay { get; set; }

        /// <summary>
        ///     Queue declaration configuration.
        /// </summary>
        public RabbitMqQueueDeclareSettings Declare { get; set; }
    }
}
