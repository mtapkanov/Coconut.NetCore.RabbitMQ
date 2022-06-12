using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Coconut.NetCore.RabbitMQ.Configuration.Options;
using Coconut.NetCore.RabbitMQ.Core.Events;
using Coconut.NetCore.RabbitMQ.Processing;
using Coconut.NetCore.RabbitMQ.Processing.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Coconut.NetCore.RabbitMQ.Internal
{
    internal class RabbitMqQueueController : IRabbitMqQueueController
    {
        private readonly IServiceProvider _provider;
        private readonly IConnection _connection;
        private readonly RabbitMqQueueOptions _queueOptions;
        private readonly RabbitMqEventBus _eventBus;
        private readonly IMessageDeserializer _deserializer;
        private readonly ILogger _logger;

        public RabbitMqQueueController(
            IServiceProvider provider,
            IConnection connection,
            RabbitMqQueueOptions queueOptions)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _queueOptions = queueOptions ?? throw new ArgumentNullException(nameof(queueOptions));
            
            _deserializer = (IMessageDeserializer)_provider.GetRequiredService(_queueOptions.DeserializerType);
            _logger = _provider.GetRequiredService<ILoggerFactory>().CreateLogger($"{nameof(RabbitMqPublisher)}<{_queueOptions.MessageType.FullName}>");
            _eventBus = _provider.GetRequiredService<RabbitMqEventBus>();
        }

        public void Run(CancellationToken cancellationToken)
        {
            var channel = CreateChannel(_connection, _queueOptions.QueueSettings.PrefetchCount);

            Declare(channel);
            Subscribe(channel, cancellationToken);
        }

        private void Declare(IModel channel)
        {
            var declareSettings = _queueOptions.QueueSettings.Declare;

            if (declareSettings is null)
                return;

            channel.QueueDeclare(
                _queueOptions.QueueSettings.Name,
                declareSettings.Durable,
                declareSettings.Exclusive,
                declareSettings.AutoDelete,
                declareSettings.Arguments);

            foreach (var bindingSetting in declareSettings.Bindings)
                channel.QueueBind(
                    _queueOptions.QueueSettings.Name,
                    bindingSetting.Exchange,
                    bindingSetting.RoutingKey,
                    bindingSetting.Arguments);
        }

        private void Subscribe(IModel channel, CancellationToken cancellationToken)
        {
            var basicConsumer = new AsyncEventingBasicConsumer(channel);

            basicConsumer.Received += async (_, basicEvent) =>
            {
                bool failed;
                Dictionary<string, object> data = null;

                try
                {
                    var message = _deserializer.Deserialize(basicEvent.Body.Span);

                    var context = (IConsumeContext)Activator.CreateInstance(typeof(ConsumeContext<>)
                        .MakeGenericType(_queueOptions.MessageType), basicEvent, message)!;

                    var messageConsumer = GetConsumer();

                    await messageConsumer.Consume(context, cancellationToken);

                    if (context.Failed)
                        _logger.LogError($"Message consumption failed. Sending back to queue. Delivery tag: {basicEvent.DeliveryTag}; Exchange: {basicEvent.Exchange}; Routing key:{basicEvent.RoutingKey}");
                    
                    else if (_logger.IsEnabled(LogLevel.Trace))
                        _logger.LogTrace($"Message consumption success. Removing from queue. Delivery tag: {basicEvent.DeliveryTag}; Exchange: {basicEvent.Exchange}; Routing key:{basicEvent.RoutingKey}");

                    failed = context.Failed;
                    data = context.Data;
                }
                catch (Exception exception) when (exception is OperationCanceledException || exception.InnerException is OperationCanceledException)
                {
                    _logger.LogWarning(exception, $"Message consumption cancelled. Sending back to queue. Delivery tag: {basicEvent.DeliveryTag}; Exchange: {basicEvent.Exchange}; Routing key:{basicEvent.RoutingKey}");

                    failed = true;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, $"Message consumption error. Sending back to queue. Delivery tag: {basicEvent.DeliveryTag}; Exchange: {basicEvent.Exchange}; Routing key:{basicEvent.RoutingKey}");

                    failed = true;
                }

                if (failed)
                {
                    channel.BasicNack(basicEvent.DeliveryTag, false, true);
                    await _eventBus.Publish(new MessageRejectedEvent(basicEvent, data), cancellationToken);

                    if (_queueOptions.QueueSettings.FailDelay > TimeSpan.Zero)
                        await Task.Delay(_queueOptions.QueueSettings.FailDelay, cancellationToken);
                }
                else
                {
                    channel.BasicAck(basicEvent.DeliveryTag, false);
                    await _eventBus.Publish(new MessageAcknowledgedEvent(basicEvent, data), cancellationToken);
                }
            };

            StartConsumer(channel, basicConsumer, _queueOptions.QueueSettings.Name, cancellationToken);
        }

        private string StartConsumer(IModel channel, IBasicConsumer consumer, string queue, CancellationToken cancellationToken) =>
            Policy.Handle<BrokerUnreachableException>()
                .WaitAndRetryForever(
                    sleepDurationProvider: WaitDurationProvider.WaitUpTo30Seconds,
                    onRetry: (exception, timespan) =>
                    {
                        _logger.LogError(exception, $"RabbitMQ consumption failed. Retry in {timespan:c}. Queue: {queue}");
                    })
                .Execute(token =>
                    {
                        token.ThrowIfCancellationRequested();
                        return channel.BasicConsume(queue, autoAck: false, consumer);
                    },
                    cancellationToken);

        private IMessageConsumer GetConsumer()
        {
            using var serviceScope = _provider.CreateScope();
            return (IMessageConsumer)serviceScope.ServiceProvider.GetRequiredService(_queueOptions.ConsumerType);
        }

        private static IModel CreateChannel(IConnection connection, ushort prefetchCount)
        {
            var model = connection.CreateModel();
            model.BasicQos(0, prefetchCount, true);
            return model;
        }
    }
}
