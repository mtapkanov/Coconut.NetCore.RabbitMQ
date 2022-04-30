using System;
using System.Collections.Generic;
using System.Threading;
using Coconut.NetCore.RabbitMQ.Configuration;
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

        private readonly Dictionary<string, List<IRabbitMqPublishController>> _publishControllers = new();

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

            if (!_publishControllers.ContainsKey(messageType))
                throw new NotSupportedException($"Message type {nameof(TMessage)} not configured to publishing in RabbitMQ bus.");

            _publishControllers[messageType]
                .ForEach(publishController => ((RabbitMqPublishController<TMessage>)publishController).Publish(message));
        }

        private void StartPublishControllers(RabbitMqOptions rabbitMqOptions, IConnection connection)
        {
            foreach (var exchangeOptions in rabbitMqOptions.RabbitMqExchangeOptions)
            {
                foreach (var publishOptions in exchangeOptions.AcceptedPublishOptions)
                {
                    var publishController = _factory.CreatePublishController(publishOptions, connection, exchangeOptions.ExchangeSettings.Name);
                    publishController.Run();

                    var messageType = publishOptions.MessageType.FullName;

                    if (!_publishControllers.ContainsKey(messageType))
                        _publishControllers.Add(messageType, new());

                    _publishControllers[messageType].Add(publishController);
                }
            }
        }
    }
}
