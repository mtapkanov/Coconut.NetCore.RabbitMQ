using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyToUse.Infrastructure.Configuration
{
    public static class AppConfiguration
    {
        private static IConfiguration Configuration { get; set; }
        public static RabbitMqSettings RabbitMqSettings { get; set; }

        public static IConfiguration UseConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMqSettings = configuration.GetSection("RabbitMQ").Get<RabbitMqSettings>();

            RabbitMqSettings = rabbitMqSettings;

            services.AddSingleton(rabbitMqSettings);

            return configuration;
        }
    }
}