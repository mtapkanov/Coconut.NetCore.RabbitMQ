using System;
using Coconut.NetCore.RabbitMQ.Processing.Internal;

namespace Coconut.NetCore.RabbitMQ.Processing
{
    /// <inheritdoc />
    public abstract class MessageSerializerBase<TMessage> : IMessageSerializer<TMessage>
    {
        /// <inheritdoc />
        public abstract ReadOnlyMemory<byte> Serialize(TMessage message);

        /// <inheritdoc />
        public ReadOnlyMemory<byte> Serialize(object message) =>
            Serialize((TMessage)message);
    }
}