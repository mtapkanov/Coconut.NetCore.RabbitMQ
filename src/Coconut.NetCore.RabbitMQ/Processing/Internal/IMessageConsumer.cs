using System.Threading;
using System.Threading.Tasks;

namespace Coconut.NetCore.RabbitMQ.Processing.Internal
{
    /// <summary>
    ///     Defines a class that is a consumer of a message. The message is wrapped in an IConsumeContext
    ///     interface to allow access to details surrounding the inbound message.
    ///     If consume context `Failed` set true or Consume method returns exception the message will be rejected.
    ///     Otherwise the message will be acknowledged.
    /// </summary>
    internal interface IMessageConsumer
    {
        /// <summary>
        ///     Consume RabbitMQ message.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task Consume(IConsumeContext context, CancellationToken cancellationToken = default);
    }
}
