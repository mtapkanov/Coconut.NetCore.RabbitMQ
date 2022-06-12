using System;
using Coconut.NetCore.RabbitMQ.Processing;

namespace Coconut.NetCore.RabbitMQ.Tools
{
    public class MessageSerializer : MessageSerializerBase<Message>
    {
        public override ReadOnlyMemory<byte> Serialize(Message message) => 
            ByteConverterHelper.ObjectToByteArray(message);
    }
}