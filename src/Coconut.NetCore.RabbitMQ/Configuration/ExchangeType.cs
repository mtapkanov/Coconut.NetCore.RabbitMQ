namespace Coconut.NetCore.RabbitMQ.Configuration;

/// <summary>
///     AMQP exchange type.
/// </summary>
public enum ExchangeType
{
    /// <summary>Routes by exact routing key match.</summary>
    Direct,

    /// <summary>Routes to every bound queue, routing key is ignored.</summary>
    Fanout,

    /// <summary>Routes by routing key pattern (<c>*</c> - one word, <c>#</c> - zero or more words).</summary>
    Topic,

    /// <summary>Routes by message headers.</summary>
    Headers
}

internal static class ExchangeTypeExtensions
{
    public static string ToAmqp(this ExchangeType type) => type switch
    {
        ExchangeType.Direct => global::RabbitMQ.Client.ExchangeType.Direct,
        ExchangeType.Fanout => global::RabbitMQ.Client.ExchangeType.Fanout,
        ExchangeType.Topic => global::RabbitMQ.Client.ExchangeType.Topic,
        ExchangeType.Headers => global::RabbitMQ.Client.ExchangeType.Headers,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown exchange type.")
    };
}
