using System.Threading;
using System.Threading.Tasks;

namespace Coconut.NetCore.RabbitMQ.Processing.Internal
{
    /// <inheritdoc />
    internal interface IMessageConsumer<TMessage> : IMessageConsumer
    {
        /// <summary>
        ///     Consume RabbitMQ message.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task Consume(ConsumeContext<TMessage> context, CancellationToken cancellationToken);
    }
}