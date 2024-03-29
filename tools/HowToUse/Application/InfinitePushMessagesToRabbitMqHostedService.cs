using AutoFixture;
using Coconut.NetCore.RabbitMQ;

namespace HowToUse.Application
{
    public class InfinitePushMessagesToRabbitMqHostedService : BackgroundService
    {
        private readonly Fixture _fixture = new();

        private readonly IRabbitMqBus _bus;

        public InfinitePushMessagesToRabbitMqHostedService(IRabbitMqBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = _fixture.Create<Message>();
                _bus.Publish(message);
            }

            return Task.CompletedTask;
        }
    }
}