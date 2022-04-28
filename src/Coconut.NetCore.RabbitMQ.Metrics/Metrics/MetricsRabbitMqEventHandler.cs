using System;
using System.Threading;
using System.Threading.Tasks;
using Coconut.NetCore.RabbitMQ.Core.Events;
using Coconut.NetCore.RabbitMQ.Core.Handlers;

namespace Coconut.NetCore.RabbitMQ.Metrics.Metrics
{
    internal class MetricsRabbitMqEventHandler : IRabbitMqEventHandler
    {
        private readonly NumberOfProcessedRabbitMqMetrics _metrics;

        public MetricsRabbitMqEventHandler(NumberOfProcessedRabbitMqMetrics numberOfProcessedRabbitMqMetrics)
        {
            _metrics = numberOfProcessedRabbitMqMetrics ?? throw new ArgumentNullException(nameof(numberOfProcessedRabbitMqMetrics));
        }

        public Task Handle(IRabbitMqEvent @event, CancellationToken cancellationToken)
        {
            switch (@event)
            {
                case MessageAcknowledgedEvent messageAcknowledged:
                    _metrics.IncrementAcknowledgedMessagesCount(messageAcknowledged.BasicEvent.Exchange, messageAcknowledged.BasicEvent.RoutingKey);
                    break;
                case MessageRejectedEvent messageRejected:
                    _metrics.IncrementRejectedMessagesCount(messageRejected.BasicEvent.Exchange, messageRejected.BasicEvent.RoutingKey);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
