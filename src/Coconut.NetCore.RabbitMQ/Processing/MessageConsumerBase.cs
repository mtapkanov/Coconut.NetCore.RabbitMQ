using System.Threading;
using System.Threading.Tasks;
using Coconut.NetCore.RabbitMQ.Processing.Internal;

namespace Coconut.NetCore.RabbitMQ.Processing
{
    /// <inheritdoc />
    public abstract class MessageConsumerBase<TMessage> : IMessageConsumer<TMessage>
    {
        /// <inheritdoc />
        public abstract Task Consume(ConsumeContext<TMessage> context, CancellationToken cancellationToken);

        /// <inheritdoc />
        public Task Consume(IConsumeContext context, CancellationToken cancellationToken = default) => 
            Consume((ConsumeContext<TMessage>) context, cancellationToken);
    }
}