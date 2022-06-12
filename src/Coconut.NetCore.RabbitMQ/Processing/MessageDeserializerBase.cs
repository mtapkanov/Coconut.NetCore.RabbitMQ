using System;
using Coconut.NetCore.RabbitMQ.Processing.Internal;

namespace Coconut.NetCore.RabbitMQ.Processing
{
    /// <inheritdoc />
    public abstract class MessageDeserializerBase<TMessage> : IMessageDeserializer<TMessage>
    {
        /// <inheritdoc />
        public abstract TMessage Deserialize(ReadOnlySpan<byte> messageBytes);

        object IMessageDeserializer.Deserialize(ReadOnlySpan<byte> messageBytes) => 
            Deserialize(messageBytes);
    }
}