using System;
using Coconut.NetCore.RabbitMQ.Processing;

namespace EasyToUse.Infrastructure.Serialization
{
    public class MessageDeserializer : MessageDeserializerBase<Message>
    {
        public override Message Deserialize(ReadOnlySpan<byte> messageBytes) => 
            ByteConverterHelper.FromByteArray<Message>(messageBytes.ToArray());
    }
}