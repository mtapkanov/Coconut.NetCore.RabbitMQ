using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Coconut.NetCore.RabbitMQ.Configuration.Options;
using Coconut.NetCore.RabbitMQ.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Coconut.NetCore.RabbitMQ.Internal
{
    /// <summary>
    ///     RabbitMQ starter hosted service.
    /// </summary>
    internal class RabbitMqStarterHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        ///     RabbitMQ starter hosted service.
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/></param>
        public RabbitMqStarterHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            InitRabbitMqEventBus(_serviceProvider);

            StartRabbitMqBus(_serviceProvider, stoppingToken);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _serviceProvider.GetRequiredService<IRabbitMqStarter>().Stop();

            return base.StopAsync(cancellationToken);
        }

        private static void InitRabbitMqEventBus(IServiceProvider serviceProvider)
        {
            var eventHandlers = serviceProvider.GetServices<IRabbitMqEventHandler>().ToArray();
            var rabbitMqEventBus = serviceProvider.GetRequiredService<RabbitMqEventBus>();

            rabbitMqEventBus.AddEventHandlers(eventHandlers);
        }

        private static Task StartRabbitMqBus(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var options = serviceProvider.GetServices<RabbitMqOptions>().ToArray();
            return serviceProvider.GetRequiredService<IRabbitMqStarter>()
                .Start(options, cancellationToken);
        }
    }
}
