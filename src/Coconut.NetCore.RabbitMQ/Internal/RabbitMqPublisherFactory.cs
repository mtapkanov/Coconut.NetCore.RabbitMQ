using System;
using Coconut.NetCore.RabbitMQ.Configuration.Options;
using Coconut.NetCore.RabbitMQ.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Coconut.NetCore.RabbitMQ.Internal
{
    internal class RabbitMqPublisherFactory
    {
        private readonly IServiceProvider _provider;
        private readonly IConnection _connection;
        private readonly RabbitMqPublishOptions _publishOptions;
        private readonly string _exchangeName;
        private readonly ILoggerFactory _loggerFactory;

        public RabbitMqPublisherFactory(
            IServiceProvider provider,
            IConnection connection,
            RabbitMqPublishOptions publishOptions,
            string exchangeName,
            ILoggerFactory loggerFactory)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _publishOptions = publishOptions ?? throw new ArgumentNullException(nameof(publishOptions));
            _exchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IRabbitMqPublisher Create()
        {
            var chanel = _connection.CreateModel();
            var logger = _loggerFactory.CreateLogger($"{nameof(RabbitMqPublisher)}<{_publishOptions.MessageType.FullName}>");
            var serializer = (IMessageSerializer)_provider.GetRequiredService(_publishOptions.SerializerType);

            return new RabbitMqPublisher(chanel, _publishOptions, serializer, _exchangeName, logger);
        }
    }
}