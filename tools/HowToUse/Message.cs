using System.Text.Json.Serialization;

namespace HowToUse;

public sealed record Message(Guid Id, string Text, DateTimeOffset CreatedAt);

[JsonSerializable(typeof(Message))]
internal sealed partial class MessageJsonContext : JsonSerializerContext;
