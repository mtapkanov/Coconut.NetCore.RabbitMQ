using System;

namespace Coconut.NetCore.RabbitMQ.Processing.Internal
{
    /// <summary>
    ///     RabbitMQ message deserializer.
    /// </summary>
    public interface IMessageDeserializer
    {
        /// <summary>
        ///     Deserialize RabbitMQ message.
        /// </summary>
        /// <param name="messageBytes">The message data to parse.</param>
        object Deserialize(ReadOnlySpan<byte> messageBytes);
    }
}
