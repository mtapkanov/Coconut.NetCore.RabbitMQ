using System;

namespace Coconut.NetCore.RabbitMQ.Processing
{
    /// <summary>
    ///     RabbitMQ message serializer.
    /// </summary>
    public interface IMessageSerializer<TMessage>
    {
        /// <summary>
        ///     Serialize RabbitMQ message.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        ReadOnlyMemory<byte> Serialize(TMessage message);
    }
}
