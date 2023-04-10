using Coconut.NetCore.RabbitMQ.Configuration.Settings;

namespace HowToUse.Infrastructure.Configuration
{
    public class RabbitMqSettings
    {
        public Uri BaseUri { get; set; } = null!;

        public RabbitMqExchangeSettings Exchange { get; set; } = null!;

        public RabbitMqQueueSettings Queue { get; set; } = null!;
    }
}