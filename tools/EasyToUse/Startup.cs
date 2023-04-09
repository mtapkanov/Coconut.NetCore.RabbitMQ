using Coconut.NetCore.RabbitMQ.Extensions;
using Coconut.NetCore.RabbitMQ.Metrics.Extensions;
using EasyToUse.Application;
using EasyToUse.Infrastructure.Configuration;
using EasyToUse.Infrastructure.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyToUse
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging();
            services.UseConfiguration(configuration);

            var rabbitMqSettings = AppConfiguration.RabbitMqSettings;

            services.AddRabbitMq(rabbitMqSettings.BaseUri, builder =>
            {
                builder.AddExchange(rabbitMqSettings.Exchange)
                    .AcceptMessages<Message, MessageSerializer>(_ => "*");

                builder.AddQueue<Message>(rabbitMqSettings.Queue)
                    .UseConsumer<MessageConsumer>()
                    .UseDeserializer<MessageDeserializer>();
            });

            services.AddMetricsAppName(nameof(EasyToUse))
                    .AddRabbitMqMetrics();
        }
        
        public void Configure(IApplicationBuilder app)
        {
            
        }
    }
}