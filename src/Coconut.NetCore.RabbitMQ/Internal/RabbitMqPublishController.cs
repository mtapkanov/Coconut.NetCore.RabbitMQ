using System;
using Coconut.NetCore.RabbitMQ.Configuration;
using Coconut.NetCore.RabbitMQ.Configuration.Options;
using Coconut.NetCore.RabbitMQ.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Coconut.NetCore.RabbitMQ.Internal
{
    internal class RabbitMqPublishController<TMessage> : IRabbitMqPublishController
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _connection;
        private readonly RabbitMqPublishOptions _publishOptions;
        private readonly string _exchangeName;
        private readonly IMessageSerializer<TMessage> _serializer;
        private readonly ILogger<RabbitMqPublishController<TMessage>> _logger;
        private IModel _channel;

        public RabbitMqPublishController(
            IServiceProvider serviceProvider,
            IConnection connection,
            RabbitMqPublishOptions publishOptions,
            string exchangeName,
            ILoggerFactory loggerFactory)
        {
            if (loggerFactory is null) throw new ArgumentNullException(nameof(loggerFactory));

            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _publishOptions = publishOptions ?? throw new ArgumentNullException(nameof(publishOptions));
            _exchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));

            _serializer = (IMessageSerializer<TMessage>)_serviceProvider.GetRequiredService(_publishOptions.SerializerType);
            _logger = loggerFactory.CreateLogger<RabbitMqPublishController<TMessage>>();
        }

        public void Run()
        {
            _channel = _connection.CreateModel();
        }

        public void Publish(TMessage message)
        {
            var routingKey = string.Empty;

            try
            {
                routingKey = _publishOptions.GetRoutingKey(message);

                var messageBytes = _serializer.Serialize(message);

                _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    body: messageBytes);

                if (_logger.IsEnabled(LogLevel.Trace))
                    _logger.LogTrace($"Message published. Exchange: {_exchangeName}; Routing key:{routingKey}");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Message publish failed. Exchange: {_exchangeName}; Routing key:{routingKey}");

                throw;
            }
        }
    }
}
