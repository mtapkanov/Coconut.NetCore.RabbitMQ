using Coconut.NetCore.RabbitMQ.Core.Events;
using Microsoft.Extensions.Logging;

namespace Coconut.NetCore.RabbitMQ.Internal;

internal static partial class Log
{
    [LoggerMessage(1, LogLevel.Information, "Connected to RabbitMQ {Host}:{Port}, vhost '{VirtualHost}'")]
    public static partial void Connected(ILogger logger, string host, int port, string virtualHost);

    [LoggerMessage(2, LogLevel.Error, "RabbitMQ {Host}:{Port} unreachable, attempt {Attempt}. Retry in {Delay}")]
    public static partial void ConnectFailed(ILogger logger, Exception exception, string host, int port, int attempt, TimeSpan delay);

    [LoggerMessage(3, LogLevel.Warning, "RabbitMQ shutdown step failed for {Host}")]
    public static partial void ShutdownStepFailed(ILogger logger, Exception exception, string host);

    [LoggerMessage(10, LogLevel.Trace, "Message published. Exchange: {Exchange}; Routing key: {RoutingKey}")]
    public static partial void MessagePublished(ILogger logger, string exchange, string routingKey);

    [LoggerMessage(11, LogLevel.Error, "Message not confirmed by broker. Exchange: {Exchange}; Routing key: {RoutingKey}; Unroutable: {IsReturn}")]
    public static partial void MessageNotConfirmed(ILogger logger, Exception exception, string exchange, string routingKey, bool isReturn);

    [LoggerMessage(12, LogLevel.Error, "Message publish failed. Exchange: {Exchange}; Routing key: {RoutingKey}")]
    public static partial void PublishFailed(ILogger logger, Exception exception, string exchange, string routingKey);

    [LoggerMessage(13, LogLevel.Error, "Message returned by broker as unroutable. Exchange: {Exchange}; Routing key: {RoutingKey}; Reply: {ReplyCode} {ReplyText}")]
    public static partial void MessageReturned(ILogger logger, string exchange, string routingKey, ushort replyCode, string replyText);

    [LoggerMessage(20, LogLevel.Information, "Consuming queue '{Queue}'. Prefetch: {PrefetchCount}; Concurrency: {Concurrency}")]
    public static partial void ConsumerStarted(ILogger logger, string queue, ushort prefetchCount, ushort concurrency);

    [LoggerMessage(21, LogLevel.Warning, "Queue '{Queue}': prefetch {PrefetchCount} is below dispatch concurrency {Concurrency}, extra dispatch slots will stay idle")]
    public static partial void PrefetchBelowConcurrency(ILogger logger, string queue, ushort prefetchCount, ushort concurrency);

    [LoggerMessage(22, LogLevel.Warning, "Queue '{Queue}' declared without bindings: it receives messages only via the default exchange")]
    public static partial void QueueWithoutBindings(ILogger logger, string queue);

    [LoggerMessage(23, LogLevel.Trace, "Message acknowledged. Queue: {Queue}; Delivery tag: {DeliveryTag}")]
    public static partial void MessageAcknowledged(ILogger logger, string queue, ulong deliveryTag);

    [LoggerMessage(24, LogLevel.Warning, "Message rejected. Queue: {Queue}; Delivery tag: {DeliveryTag}; Exchange: {Exchange}; Routing key: {RoutingKey}; Requeued: {Requeued}")]
    public static partial void MessageRejected(ILogger logger, Exception? exception, string queue, ulong deliveryTag, string exchange, string routingKey, bool requeued);

    [LoggerMessage(25, LogLevel.Warning, "Ack/nack failed, the broker will redeliver the message. Queue: {Queue}; Delivery tag: {DeliveryTag}")]
    public static partial void SettleFailed(ILogger logger, Exception exception, string queue, ulong deliveryTag);

    [LoggerMessage(30, LogLevel.Error, "RabbitMQ event handler {Handler} failed on {Event}")]
    public static partial void EventHandlerFailed(ILogger logger, Exception exception, string handler, IRabbitMqEvent @event);
}
