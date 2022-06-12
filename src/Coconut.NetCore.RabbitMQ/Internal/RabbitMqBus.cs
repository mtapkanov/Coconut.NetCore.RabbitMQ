using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Coconut.NetCore.RabbitMQ.Configuration.Options;
using Coconut.NetCore.RabbitMQ.Processing.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Coconut.NetCore.RabbitMQ.Internal
{
    internal class RabbitMqBus : IRabbitMqBus
    {
        private readonly IServiceProvider _provider;
        private readonly List<RabbitMqUnit> _rabbitMqUnits = new();

        private readonly Dictionary<string, List<IRabbitMqPublisher>> _publishers = new();

        public RabbitMqBus(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public void Start(IList<RabbitMqOptions> rabbitMqUnitsOptions, CancellationToken cancellationToken)
        {
            foreach (var rabbitMqOptions in rabbitMqUnitsOptions)
            {
                var rabbitMqUnit = _provider.GetRequiredService<RabbitMqUnit>();

                _rabbitMqUnits.Add(rabbitMqUnit);

                rabbitMqUnit.Start(rabbitMqOptions, cancellationToken);

                StartPublishers(rabbitMqOptions, rabbitMqUnit.Connection);
            }
        }

        public void Stop()
        {
            _rabbitMqUnits.ForEach(rabbitMqUnit => rabbitMqUnit.Stop());
        }

        public void Publish<TMessage>(TMessage message)
        {
            var messageType = typeof(TMessage).FullName!;
            
            if (!_publishers.TryGetValue(messageType, out var publishers))
                throw new NotSupportedException($"Message type {nameof(TMessage)} not configured to publishing in RabbitMQ bus.");

            foreach (var publisher in publishers)
                publisher.Publish(message);
        }

        private void StartPublishers(RabbitMqOptions rabbitMqOptions, IConnection connection)
        {
            foreach (var (exchangeName, publishOptions) in GetPublishOptions(rabbitMqOptions))
            {
                var messageType = publishOptions.MessageType.FullName!;
                
                var chanel = connection.CreateModel();
                var logger = _provider.GetRequiredService<ILoggerFactory>().CreateLogger($"{nameof(RabbitMqPublisher)}<{publishOptions.MessageType.FullName}>");
                var serializer = (IMessageSerializer)_provider.GetRequiredService(publishOptions.SerializerType);

                var publisher = new RabbitMqPublisher(chanel, publishOptions, serializer, exchangeName, logger);

                if (!_publishers.ContainsKey(messageType))
                    _publishers.Add(messageType, new List<IRabbitMqPublisher>());

                _publishers[messageType].Add(publisher);
            }
        }
        
        private static IEnumerable<(string exchangeName, RabbitMqPublishOptions publishOptions)> GetPublishOptions(RabbitMqOptions rabbitMqOptions) =>
            rabbitMqOptions.RabbitMqExchangeOptions
                .SelectMany(exchangeOptions => exchangeOptions.AcceptedPublishOptions
                    .Select(publishOptions => (exchangeOptions.ExchangeSettings.Name, publishOptions)));
    }
}
