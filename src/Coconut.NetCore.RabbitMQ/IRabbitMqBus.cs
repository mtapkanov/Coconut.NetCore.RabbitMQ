namespace Coconut.NetCore.RabbitMQ
{
    /// <summary>
    ///     RabbitMQ message bus.
    /// </summary>
    public interface IRabbitMqBus
    {
        /// <summary>
        ///     Publish message to RabbitMQ bus.
        /// </summary>
        /// <param name="message">Messages that will be distributed through the exchanges.</param>
        void Publish<TMessage>(TMessage message);
    }
}
