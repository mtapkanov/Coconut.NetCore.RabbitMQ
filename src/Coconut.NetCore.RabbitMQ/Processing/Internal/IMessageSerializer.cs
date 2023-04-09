using System;

namespace Coconut.NetCore.RabbitMQ.Processing.Internal
{
    /// <summary>
    ///     RabbitMQ message serializer.
    /// </summary>
    internal interface IMessageSerializer
    {
        /// <summary>
        ///     Serialize RabbitMQ message.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        ReadOnlyMemory<byte> Serialize(object message);
    }
}
