using System;

namespace Coconut.NetCore.RabbitMQ.Processing.Internal
{
    /// <inheritdoc />
    internal interface IMessageSerializer<in TMessage> : IMessageSerializer
    {
        /// <summary>
        ///     Serialize RabbitMQ message.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        ReadOnlyMemory<byte> Serialize(TMessage message);
    }
}