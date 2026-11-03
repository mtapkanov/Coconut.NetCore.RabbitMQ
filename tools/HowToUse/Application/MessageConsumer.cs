using Coconut.NetCore.RabbitMQ.Processing;

namespace HowToUse.Application;

public sealed class MessageConsumer(ILogger<MessageConsumer> logger) : IMessageConsumer<Message>
{
    private static long _consumed;

    public Task ConsumeAsync(ConsumeContext<Message> context, CancellationToken cancellationToken)
    {
        var count = Interlocked.Increment(ref _consumed);
        var properties = context.Delivery.BasicProperties;
        logger.LogInformation(
            "#{Count} {MessageId} amqp-id: {AmqpMessageId}; correlation: {CorrelationId}; {ContentType}; persistent: {Persistent}; redelivered: {Redelivered}",
            count, context.Message.Id, properties.MessageId, properties.CorrelationId, properties.ContentType, properties.Persistent, context.Delivery.Redelivered);

        context.Data["message_id"] = context.Message.Id;
        return Task.CompletedTask;
    }
}
