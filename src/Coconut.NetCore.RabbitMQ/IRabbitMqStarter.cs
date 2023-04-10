using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Coconut.NetCore.RabbitMQ.Configuration.Options;

namespace Coconut.NetCore.RabbitMQ
{
    internal interface IRabbitMqStarter
    {
        /// <summary>
        ///      Starts RabbitMQ message bus.
        /// </summary>
        /// <param name="rabbitMqUnitsOptions">RabbitMQ unit options collection.</param>
        /// <param name="cancellationToken">Cancellation token to stop RabbitMQ interactions.</param>
        Task Start(IList<RabbitMqOptions> rabbitMqUnitsOptions, CancellationToken cancellationToken);

        /// <summary>
        ///      Stop RabbitMQ message bus.
        /// </summary>
        Task Stop();
    }
}