using System.Collections.Generic;
using System.Threading;
using Coconut.NetCore.RabbitMQ.Configuration;
using Coconut.NetCore.RabbitMQ.Configuration.Options;

namespace Coconut.NetCore.RabbitMQ
{
    /// <summary>
    ///     RabbitMQ message bus.
    /// </summary>
    public interface IRabbitMqBus
    {
        /// <summary>
        ///      Starts RabbitMQ message bus.
        /// </summary>
        /// <param name="rabbitMqUnitsOptions">RabbitMQ unit options collection.</param>
        /// <param name="cancellationToken">Cancellation token to stop RabbitMQ interactions.</param>
        void Start(IList<RabbitMqOptions> rabbitMqUnitsOptions, CancellationToken cancellationToken);

        /// <summary>
        ///      Stop RabbitMQ message bus.
        /// </summary>
        void Stop();

        /// <summary>
        ///     Publish message to RabbitMQ bus.
        /// </summary>
        /// <param name="message">Messages that will be distributed through the exchanges.</param>
        void Publish<TMessage>(TMessage message);
    }
}
