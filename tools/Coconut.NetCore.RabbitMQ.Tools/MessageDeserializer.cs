using System;
using Coconut.NetCore.RabbitMQ.Processing;

namespace Coconut.NetCore.RabbitMQ.Tools
{
    public class MessageDeserializer : MessageDeserializerBase<Message>
    {
        public override Message Deserialize(ReadOnlySpan<byte> messageBytes) => 
            ByteConverterHelper.FromByteArray<Message>(messageBytes.ToArray());
    }
}