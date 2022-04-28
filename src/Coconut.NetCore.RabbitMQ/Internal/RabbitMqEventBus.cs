using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Coconut.NetCore.RabbitMQ.Core.Events;
using Coconut.NetCore.RabbitMQ.Core.Handlers;
using Microsoft.Extensions.Logging;

namespace Coconut.NetCore.RabbitMQ.Internal
{
    internal class RabbitMqEventBus
    {
        private readonly ILogger<RabbitMqEventBus> _logger;

        private List<IRabbitMqEventHandler> _eventHandlers;

        public RabbitMqEventBus(ILogger<RabbitMqEventBus> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void AddEventHandlers(params IRabbitMqEventHandler[] eventHandlers)
        {
            _eventHandlers ??= new List<IRabbitMqEventHandler>();
            _eventHandlers.AddRange(eventHandlers);
        }

        public async Task Publish(IRabbitMqEvent @event, CancellationToken cancellationToken)
        {
            foreach (var eventHandler in _eventHandlers)
            {
                try
                {
                    await eventHandler.Handle(@event, cancellationToken);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, $"RabbitMQ event handing error. Event info: {@event.ToString()}");
                }
            }
        }
    }
}
