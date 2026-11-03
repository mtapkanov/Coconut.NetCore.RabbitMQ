using Coconut.NetCore.RabbitMQ.Core.Events;
using Coconut.NetCore.RabbitMQ.Core.Handlers;
using Microsoft.Extensions.Logging;

namespace Coconut.NetCore.RabbitMQ.Internal;

internal sealed class RabbitMqEventBus(IEnumerable<IRabbitMqEventHandler> handlers, ILogger<RabbitMqEventBus> logger)
{
    private readonly IRabbitMqEventHandler[] _handlers = [.. handlers];

    public ValueTask PublishAsync(IRabbitMqEvent @event, CancellationToken cancellationToken) =>
        _handlers.Length == 0 ? ValueTask.CompletedTask : PublishCoreAsync(@event, cancellationToken);

    private async ValueTask PublishCoreAsync(IRabbitMqEvent @event, CancellationToken cancellationToken)
    {
        foreach (var handler in _handlers)
        {
            try
            {
                await handler.HandleAsync(@event, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Log.EventHandlerFailed(logger, exception, handler.GetType().Name, @event);
            }
        }
    }
}
