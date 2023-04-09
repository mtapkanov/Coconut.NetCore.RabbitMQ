using System;

namespace Coconut.NetCore.RabbitMQ.Processing.Internal
{
    /// <summary>
    ///     RabbitMQ message deserializer.
    /// </summary>
    public interface IMessageDeserializer<out TMessage> : IMessageDeserializer
    {
        /// <summary>
        ///     Deserialize RabbitMQ message.
        /// </summary>
        /// <param name="messageBytes">The message data to parse.</param>
        new TMessage Deserialize(ReadOnlySpan<byte> messageBytes);
    }
}