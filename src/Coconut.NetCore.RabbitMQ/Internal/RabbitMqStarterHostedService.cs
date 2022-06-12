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
            var rabbitMqBus = _serviceProvider.GetRequiredService<IRabbitMqBus>();
            rabbitMqBus.Stop();

            return base.StopAsync(cancellationToken);
        }

        private static void InitRabbitMqEventBus(IServiceProvider serviceProvider)
        {
            var eventHandlers = serviceProvider.GetServices<IRabbitMqEventHandler>().ToArray();
            var rabbitMqEventBus = serviceProvider.GetRequiredService<RabbitMqEventBus>();

            rabbitMqEventBus.AddEventHandlers(eventHandlers);
        }

        private static void StartRabbitMqBus(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var rabbitMqUnitsOptions = serviceProvider.GetServices<RabbitMqOptions>().ToArray();
            var rabbitMqBus = serviceProvider.GetRequiredService<IRabbitMqBus>();

            rabbitMqBus.Start(rabbitMqUnitsOptions, cancellationToken);
        }
    }
}
