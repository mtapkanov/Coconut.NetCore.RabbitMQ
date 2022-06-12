using System.Threading;
using System.Threading.Tasks;
using Coconut.NetCore.RabbitMQ.Core.Events;

namespace Coconut.NetCore.RabbitMQ.Core.Handlers
{
    /// <summary>
    ///     RabbitMQ event handler.
    /// </summary>
    public interface IRabbitMqEventHandler
    {
        /// <summary>
        ///     Handles the RabbitMQ events.
        /// </summary>
        Task Handle(IRabbitMqEvent @event, CancellationToken cancellationToken = default);
    }
}