using Coconut.NetCore.RabbitMQ.Configuration.Settings;

namespace HowToUse.Infrastructure.Configuration;

public sealed class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    public required Uri BaseUri { get; init; }

    public required RabbitMqExchangeSettings Exchange { get; init; }

    public required RabbitMqQueueSettings Queue { get; init; }
}
