using System;
using Coconut.NetCore.RabbitMQ.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Coconut.NetCore.RabbitMQ.Internal
{
    internal class RabbitMqFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMqEventBus _eventBus;
        private readonly ILoggerFactory _loggerFactory;

        public RabbitMqFactory(IServiceProvider serviceProvider, RabbitMqEventBus eventBus, ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IRabbitMqPublishController CreatePublishController(RabbitMqPublishOptions publishOptions, IConnection connection, string exchangeName)
        {
            Type controllerType = typeof(RabbitMqPublishController<>).MakeGenericType(publishOptions.MessageType);
            return (IRabbitMqPublishController)Activator.CreateInstance(controllerType, _serviceProvider, connection, publishOptions, exchangeName, _loggerFactory);
        }

        public IRabbitMqQueueController CreateQueueController(RabbitMqQueueOptions queueOptions, IConnection connection)
        {
            Type controllerType = typeof(RabbitMqQueueController<>).MakeGenericType(queueOptions.MessageType);
            return (IRabbitMqQueueController)Activator.CreateInstance(controllerType, _serviceProvider, connection, queueOptions, _eventBus, _loggerFactory);
        }
    }
}
