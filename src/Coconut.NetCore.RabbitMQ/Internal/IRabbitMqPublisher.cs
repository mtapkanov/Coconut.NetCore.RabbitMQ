namespace Coconut.NetCore.RabbitMQ.Internal
{
    internal interface IRabbitMqPublisher
    {
        void Publish(object message);
    }
}
