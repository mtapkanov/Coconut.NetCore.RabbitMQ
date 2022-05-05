using System;
using Coconut.NetCore.RabbitMQ.Configuration.Options;
using Coconut.NetCore.RabbitMQ.Processing;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Coconut.NetCore.RabbitMQ.Internal
{
    internal class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly IModel _channel;
        private readonly RabbitMqPublishOptions _options;
        private readonly IMessageSerializer _serializer;
        private readonly string _exchangeName;
        private readonly ILogger _logger;

        public RabbitMqPublisher(IModel channel, RabbitMqPublishOptions options, IMessageSerializer serializer, string exchangeName, ILogger logger)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _exchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Publish(object message)
        {
            var routingKey = string.Empty;

            try
            {
                routingKey = _options.GetRoutingKey(message);

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
