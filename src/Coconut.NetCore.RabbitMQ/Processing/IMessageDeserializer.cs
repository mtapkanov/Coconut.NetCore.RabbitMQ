using System;

namespace Coconut.NetCore.RabbitMQ.Processing
{
    /// <summary>
    ///     RabbitMQ message deserializer.
    /// </summary>
    public interface IMessageDeserializer<out TMessage>
    {
        /// <summary>
        ///     Deserialize RabbitMQ message.
        /// </summary>
        /// <param name="messageBytes">The message data to parse.</param>
        TMessage Deserialize(ReadOnlySpan<byte> messageBytes);
    }
}
