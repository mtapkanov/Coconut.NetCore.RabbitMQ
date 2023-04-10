using Coconut.NetCore.RabbitMQ.Processing;

namespace HowToUse.Infrastructure.Serialization
{
    public class MessageSerializer : MessageSerializerBase<Message>
    {
        public override ReadOnlyMemory<byte> Serialize(Message message) =>
            ByteConverterHelper.ObjectToByteArray(message);
    }
}