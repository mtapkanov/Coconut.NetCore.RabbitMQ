using Coconut.NetCore.RabbitMQ;
using RabbitMQ.Client.Exceptions;

namespace HowToUse.Application;

/// <summary>
///     Publishes a message every 100 ms. Starts after the RabbitMQ hosted service, so the bus is already connected.
/// </summary>
public sealed class MessageProducer(IRabbitMqBus bus, TimeProvider timeProvider, ILogger<MessageProducer> logger) : BackgroundService
{
    public const string RoutingKey = "messages";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100), timeProvider);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var message = new Message(Guid.CreateVersion7(), "Hello from HowToUse", timeProvider.GetUtcNow());

            try
            {
                await bus.PublishAsync(message, stoppingToken);
            }
            catch (PublishException exception)
            {
                logger.LogWarning(exception, "Message {MessageId} not confirmed by broker (unroutable: {IsReturn})", message.Id, exception.IsReturn);
            }
        }
    }
}
