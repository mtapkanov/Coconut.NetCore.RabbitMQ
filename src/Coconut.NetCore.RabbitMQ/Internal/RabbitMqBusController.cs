using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Coconut.NetCore.RabbitMQ.Configuration.Options;
using Coconut.NetCore.RabbitMQ.Processing.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Coconut.NetCore.RabbitMQ.Internal
{
    internal class RabbitMqBusController : IRabbitMqBusController
    {
        private readonly IServiceProvider _provider;
        private readonly List<RabbitMqUnit> _rabbitMqUnits = new();
        private readonly PublisherCache _publisherCache;

        public RabbitMqBusController(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _publisherCache = _provider.GetRequiredService<PublisherCache>();
        }

        public Task Start(IList<RabbitMqOptions> rabbitMqUnitsOptions, CancellationToken cancellationToken)
        {
            foreach (var rabbitMqOptions in rabbitMqUnitsOptions)
            {
                var rabbitMqUnit = _provider.GetRequiredService<RabbitMqUnit>();

                _rabbitMqUnits.Add(rabbitMqUnit);

                rabbitMqUnit.Start(rabbitMqOptions, cancellationToken);

                StartPublishers(rabbitMqOptions, rabbitMqUnit.Connection);
            }
            
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _rabbitMqUnits.ForEach(rabbitMqUnit => rabbitMqUnit.Stop());
            return Task.CompletedTask;
        }
        
        private void StartPublishers(RabbitMqOptions rabbitMqOptions, IConnection connection)
        {
            foreach (var (exchangeName, publishOptions) in GetPublishOptions(rabbitMqOptions))
            {
                var chanel = connection.CreateModel();
                var logger = _provider.GetRequiredService<ILoggerFactory>().CreateLogger($"{nameof(RabbitMqPublisher)}<{publishOptions.MessageType.FullName}>");
                var serializer = (IMessageSerializer)_provider.GetRequiredService(publishOptions.SerializerType);

                var publisher = new RabbitMqPublisher(chanel, publishOptions, serializer, exchangeName, logger);
                _publisherCache.AddPublisher(publishOptions.MessageType, publisher);
            }
        }
        
        private static IEnumerable<(string exchangeName, RabbitMqPublishOptions publishOptions)> GetPublishOptions(RabbitMqOptions rabbitMqOptions) =>
            rabbitMqOptions.RabbitMqExchangeOptions
                .SelectMany(exchangeOptions => exchangeOptions.AcceptedPublishOptions
                    .Select(publishOptions => (exchangeOptions.ExchangeSettings.Name, publishOptions)));
    }
}