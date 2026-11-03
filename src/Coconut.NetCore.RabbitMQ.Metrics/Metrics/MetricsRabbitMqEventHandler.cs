using Coconut.NetCore.RabbitMQ.Core.Events;
using Coconut.NetCore.RabbitMQ.Core.Handlers;

namespace Coconut.NetCore.RabbitMQ.Metrics.Metrics;

internal sealed class MetricsRabbitMqEventHandler(NumberOfProcessedRabbitMqMetrics metrics) : IRabbitMqEventHandler
{
    public ValueTask HandleAsync(IRabbitMqEvent @event, CancellationToken cancellationToken)
    {
        switch (@event)
        {
            case MessageAcknowledgedEvent { Exchange: var exchange, RoutingKey: var routingKey }:
                metrics.IncrementAcknowledged(exchange, routingKey);
                break;
            case MessageRejectedEvent { Exchange: var exchange, RoutingKey: var routingKey }:
                metrics.IncrementRejected(exchange, routingKey);
                break;
        }

        return ValueTask.CompletedTask;
    }
}
