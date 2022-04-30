using System;
using Coconut.NetCore.RabbitMQ.Configuration.Settings;

namespace Coconut.NetCore.RabbitMQ.Configuration.Options
{
    /// <summary>
    ///     RabbitMQ queue options.
    /// </summary>
    public class RabbitMqQueueOptions
    {
        /// <summary>
        ///     RabbitMQ queue configuration.
        /// </summary>
        public RabbitMqQueueSettings QueueSettings { get; }

        /// <summary>
        ///     Message type.
        /// </summary>
        public Type MessageType { get; set; }

        /// <summary>
        ///     RabbitMQ message deserializer type.
        /// </summary>
        public Type DeserializerType { get; }

        /// <summary>
        ///     RabbitMQ consumer type.
        /// </summary>
        public Type ConsumerType { get; }

        /// <summary>
        ///     Creates RabbitMQ queue options.
        /// </summary>
        /// <param name="queueSettings">RabbitMQ queue configuration.</param>
        /// <param name="messageType"></param>
        /// <param name="deserializerType">RabbitMQ message deserializer type.</param>
        /// <param name="consumerType">RabbitMQ consumer type.</param>
        public RabbitMqQueueOptions(RabbitMqQueueSettings queueSettings, Type messageType, Type deserializerType, Type consumerType)
        {
            MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
            QueueSettings = queueSettings ?? throw new ArgumentNullException(nameof(queueSettings));
            DeserializerType = deserializerType ?? throw new ArgumentNullException(nameof(deserializerType));
            ConsumerType = consumerType ?? throw new ArgumentNullException(nameof(consumerType));
        }
    }
}
