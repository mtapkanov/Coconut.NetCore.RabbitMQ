using System;

namespace Coconut.NetCore.RabbitMQ.Internal
{
    internal class RabbitMqBus : IRabbitMqBus
    {
        private readonly PublisherCache _publisherCache;

        public RabbitMqBus(PublisherCache publisherCache)
        {
            _publisherCache = publisherCache ?? throw new ArgumentNullException(nameof(publisherCache));
        }

        public void Publish<TMessage>(TMessage message)
        {
            _publisherCache.GetPublishers(typeof(TMessage))
                .ForEach(publisher => publisher.Publish(message));
        }
    }
}
