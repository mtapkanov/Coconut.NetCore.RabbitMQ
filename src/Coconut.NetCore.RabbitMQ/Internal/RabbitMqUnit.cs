using System;
using System.Linq;
using System.Threading;
using Coconut.NetCore.RabbitMQ.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Coconut.NetCore.RabbitMQ.Internal
{
    internal class RabbitMqUnit
    {
        private readonly RabbitMqFactory _factory;
        private readonly ILogger<RabbitMqUnit> _logger;

        public IConnection Connection { get; private set; }

        public RabbitMqUnit(RabbitMqFactory factory, ILogger<RabbitMqUnit> logger)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Start(RabbitMqOptions options, CancellationToken cancellationToken)
        {
            Connection = CreateConnection(options.Uri, cancellationToken);

            if (options.RabbitMqExchangeOptions.Any())
                foreach (var exchangeOptions in options.RabbitMqExchangeOptions)
                {
                    DeclareExchange(exchangeOptions);
                }

            if (options.RabbitMqQueueOptions.Any())
                foreach (var queueOptions in options.RabbitMqQueueOptions)
                {
                    var queueController = _factory.CreateQueueController(queueOptions, Connection);
                    queueController.Run(cancellationToken);
                }
        }

        public void Stop()
        {
            Connection?.Close();
        }

        private IConnection CreateConnection(Uri uri, CancellationToken cancellationToken)
        {
            var connectionFactory = new ConnectionFactory()
            {
                Uri = uri,
                DispatchConsumersAsync = true
            };

            return Policy.Handle<BrokerUnreachableException>()
                .WaitAndRetryForever(
                    sleepDurationProvider: WaitDurationProvider.WaitUpTo30Seconds,
                    onRetry: (exception, timespan) => _logger.LogError(exception, $"RabbitMQ connecion failed. Retry in {timespan:c}. URI: {uri}"))
                .Execute(token =>
                    {
                        token.ThrowIfCancellationRequested();
                        return connectionFactory.CreateConnection();
                    },
                    cancellationToken);
        }

        private void DeclareExchange(RabbitMqExchangeOptions exchangeOptions)
        {
            using var channel = Connection.CreateModel();

            var declareSettings = exchangeOptions.ExchangeSettings.Declare;

            if (declareSettings is not null)
                channel.ExchangeDeclare(
                    exchangeOptions.ExchangeSettings.Name,
                    $"{declareSettings.Type}".ToLower(),
                    declareSettings.Durable,
                    declareSettings.AutoDelete,
                    declareSettings.Arguments);
        }
    }
}
