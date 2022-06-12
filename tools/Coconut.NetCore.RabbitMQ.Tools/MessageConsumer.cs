using System;
using System.Threading;
using System.Threading.Tasks;
using Coconut.NetCore.RabbitMQ.Processing;

namespace Coconut.NetCore.RabbitMQ.Tools
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