using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Coconut.NetCore.RabbitMQ.Internal
{
    internal class PublisherCache
    {
        private readonly ConcurrentDictionary<Type, IList<IRabbitMqPublisher>> _cache = new();

        public void AddPublisher(Type type, IRabbitMqPublisher publisher)
        {
            _cache.AddOrUpdate(type,
                addValueFactory: _ => new List<IRabbitMqPublisher> { publisher },
                updateValueFactory: (_, list) =>
                {
                    list.Add(publisher);
                    return list;
                });
        }

        public List<IRabbitMqPublisher> GetPublishers(Type type)
        {
            if (!_cache.TryGetValue(type, out var publishers))
                throw new NotSupportedException($"Message type {type.FullName} not configured to publishing in RabbitMQ bus.");

            return publishers as List<IRabbitMqPublisher>;
        }
    }
}