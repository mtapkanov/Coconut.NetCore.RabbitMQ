using System.Text.Json;
using Coconut.NetCore.RabbitMQ.Processing;

namespace HowToUse.Infrastructure.Serialization;

/// <summary>
///     One class for both directions: registered once as a singleton and used by the exchange and the queue.
///     Source-generated System.Text.Json metadata, no runtime reflection.
/// </summary>
public sealed class MessageJsonSerializer : IMessageSerializer<Message>, IMessageDeserializer<Message>
{
    public string ContentType => "application/json";

    public ReadOnlyMemory<byte> Serialize(Message message) =>
        JsonSerializer.SerializeToUtf8Bytes(message, MessageJsonContext.Default.Message);

    public Message Deserialize(ReadOnlySpan<byte> body) =>
        JsonSerializer.Deserialize(body, MessageJsonContext.Default.Message)
        ?? throw new JsonException("Message body is JSON null.");
}
