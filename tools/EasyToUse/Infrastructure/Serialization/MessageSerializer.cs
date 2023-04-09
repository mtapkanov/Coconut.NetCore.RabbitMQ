using System;
using Coconut.NetCore.RabbitMQ.Processing;

namespace EasyToUse.Infrastructure.Serialization
{
    public class MessageSerializer : MessageSerializerBase<Message>
    {
        public override ReadOnlyMemory<byte> Serialize(Message message) => 
            ByteConverterHelper.ObjectToByteArray(message);
    }
}