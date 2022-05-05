using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Coconut.NetCore.RabbitMQ.Configuration.Options;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Coconut.NetCore.RabbitMQ.Internal
{
    internal class RabbitMqBus : IRabbitMqBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMqFactory _factory;
        private readonly List<RabbitMqUnit> _rabbitMqUnits = new();

        private readonly Dictionary<string, List<IRabbitMqPublisher>> _publishControllers = new();

        public RabbitMqBus(IServiceProvider serviceProvider, RabbitMqFactory factory)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public void Start(IList<RabbitMqOptions> rabbitMqUnitsOptions, CancellationToken cancellationToken)
        {
            foreach (var rabbitMqOptions in rabbitMqUnitsOptions)
            {
                var rabbitMqUnit = _serviceProvider.GetRequiredService<RabbitMqUnit>();

                _rabbitMqUnits.Add(rabbitMqUnit);

                rabbitMqUnit.Start(rabbitMqOptions, cancellationToken);

                StartPublishControllers(rabbitMqOptions, rabbitMqUnit.Connection);
            }
        }

        public void Stop()
        {
            _rabbitMqUnits.ForEach(rabbitMqUnit => rabbitMqUnit.Stop());
        }

        public void Publish<TMessage>(TMessage message)
        {
            var messageType = typeof(TMessage).FullName;
            
            if (!_publishControllers.TryGetValue(messageType, out var publishers))
                throw new NotSupportedException($"Message type {nameof(TMessage)} not configured to publishing in RabbitMQ bus.");

            foreach (var publisher in publishers)
                publisher.Publish(message);
        }

        private void StartPublishControllers(RabbitMqOptions rabbitMqOptions, IConnection connection)
        {
            foreach (var (exchangeName, publishOptions) in GetPublishOptions(rabbitMqOptions))
            {
                var messageType = publishOptions.MessageType.FullName!;
                var publisher = _factory.CreatePublisher(publishOptions, connection, exchangeName);
                
                if (!_publishControllers.ContainsKey(messageType))
                    _publishControllers.Add(messageType, new List<IRabbitMqPublisher>());

                _publishControllers[messageType].Add(publisher);
            }
        }
        
        private static IEnumerable<(string exchangeName, RabbitMqPublishOptions publishOptions)> GetPublishOptions(RabbitMqOptions rabbitMqOptions) =>
            rabbitMqOptions.RabbitMqExchangeOptions
                .SelectMany(exchangeOptions => exchangeOptions.AcceptedPublishOptions
                    .Select(publishOptions => (exchangeOptions.ExchangeSettings.Name, publishOptions)));
    }
}
