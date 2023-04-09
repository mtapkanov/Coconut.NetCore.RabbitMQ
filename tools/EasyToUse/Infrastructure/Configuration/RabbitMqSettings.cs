using System;
using Coconut.NetCore.RabbitMQ.Configuration.Settings;

namespace EasyToUse.Infrastructure.Configuration
{
    public class RabbitMqSettings
    {
        public Uri BaseUri { get; set; }

        public RabbitMqExchangeSettings Exchange { get; set; }
        
        public RabbitMqQueueSettings Queue { get; set; }
    }
}