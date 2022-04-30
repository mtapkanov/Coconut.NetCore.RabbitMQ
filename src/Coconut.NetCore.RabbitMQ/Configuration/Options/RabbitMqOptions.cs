using System;

namespace Coconut.NetCore.RabbitMQ.Configuration.Options
{
    /// <summary>
    ///     RabbitMQ unit options.
    /// </summary>
    public class RabbitMqOptions
    {
        /// <summary>
        ///     RabbitMQ AMQP URI containing credentials.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        ///     RabbitMQ exchanges options.
        /// </summary>
        public RabbitMqExchangeOptions[] RabbitMqExchangeOptions { get; }

        /// <summary>
        ///     RabbitMQ queues options.
        /// </summary>
        public RabbitMqQueueOptions[] RabbitMqQueueOptions { get; }

        /// <summary>
        ///     Creates RabbitMQ unit options.
        /// </summary>
        /// <param name="uri">RabbitMQ AMQP URI containing credentials.</param>
        /// <param name="rabbitMqExchangeOptions">RabbitMQ exchanges options.</param>
        /// <param name="rabbitMqQueueOptions">RabbitMQ queues options.</param>
        public RabbitMqOptions(Uri uri, RabbitMqExchangeOptions[] rabbitMqExchangeOptions, RabbitMqQueueOptions[] rabbitMqQueueOptions)
        {
            Uri = uri;
            RabbitMqExchangeOptions = rabbitMqExchangeOptions;
            RabbitMqQueueOptions = rabbitMqQueueOptions;
        }
    }
}
