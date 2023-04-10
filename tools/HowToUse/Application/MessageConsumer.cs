using Coconut.NetCore.RabbitMQ.Processing;

namespace HowToUse.Application
{
    public class MessageConsumer : MessageConsumerBase<Message>
    {
        private static int _countMessage = 0;
        public override Task Consume(ConsumeContext<Message> context, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _countMessage);
            Console.WriteLine($"{_countMessage} {context.Message}");
            return Task.CompletedTask;
        }
    }
}