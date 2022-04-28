namespace Coconut.NetCore.RabbitMQ.Configuration
{
    /// <summary>
    ///     Exchange type <see href="https://www.rabbitmq.com/tutorials/amqp-concepts.html"/>
    /// </summary>
    public enum ExchangeType
    {
        /// <summary>
        ///     A direct exchange delivers messages to queues based on the message routing key.
        /// /// </summary>
        Direct,

        /// <summary>
        ///     A fanout exchange routes messages to all of the queues that are bound to it and the routing key is ignored.
        /// </summary>
        Fanout,

        /// <summary>
        ///     Topic exchanges route messages to one or many queues based on matching between a message routing key and the pattern that was used to bind a queue to an exchange.
        /// </summary>
        Topic,

        /// <summary>
        ///     A headers exchange is designed for routing on multiple attributes that are more easily expressed as message headers than a routing key.
        /// </summary>
        Headers
    }
}
