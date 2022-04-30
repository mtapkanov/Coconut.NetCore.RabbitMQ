using System;

namespace Coconut.NetCore.RabbitMQ.Configuration.Options
{
    /// <summary>
    ///     RabbitMQ exchange publish options.
    /// </summary>
    public class RabbitMqPublishOptions
    {
        /// <summary>
        ///     Type messages that will be distributed through the exchange.
        /// </summary>
        public Type MessageType { get; }

        /// <summary>
        ///     RabbitMQ message serializer type.
        /// </summary>
        public Type SerializerType { get; }

        /// <summary>
        ///     RabbitMQ message routing key getter.
        /// </summary>
        public Func<object, string> GetRoutingKey { get; }

        /// <summary>
        ///     Creates RabbitMQ exchange publish options.
        /// </summary>
        /// <param name="messageType">Type messages that will be distributed through the exchange.</param>
        /// <param name="serializerType">RabbitMQ message serializer type.</param>
        /// <param name="getRoutingKey">RabbitMQ message routing key getter.</param>
        public RabbitMqPublishOptions(Type messageType, Type serializerType, Func<object, string> getRoutingKey)
        {
            MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
            SerializerType = serializerType ?? throw new ArgumentNullException(nameof(serializerType));
            GetRoutingKey = getRoutingKey ?? throw new ArgumentNullException(nameof(getRoutingKey));
        }
    }
}
