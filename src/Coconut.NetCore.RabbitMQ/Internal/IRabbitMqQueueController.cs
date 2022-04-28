using System.Threading;

namespace Coconut.NetCore.RabbitMQ.Internal
{
    internal interface IRabbitMqQueueController
    {
        void Run(CancellationToken cancellationToken);
    }
}
