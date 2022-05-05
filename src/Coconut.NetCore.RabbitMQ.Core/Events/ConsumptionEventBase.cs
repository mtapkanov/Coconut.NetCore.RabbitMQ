using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client.Events;

namespace Coconut.NetCore.RabbitMQ.Core.Events
{
    /// <summary>
    ///     RabbitMQ message consumption event.
    /// </summary>
    public abstract class ConsumptionEventBase : IRabbitMqEvent
    {
        /// <summary>
        ///     <see cref="BasicDeliverEventArgs"/>
        /// </summary>
        /// <value></value>
        public BasicDeliverEventArgs BasicEvent { get; }

        /// <inheritdoc />
        public Dictionary<string, object> Data { get; }

        /// <inheritdoc cref="IRabbitMqEvent" />
        public override string ToString() =>
            string.Join("; ", Data.Select(x => $"key: {x.Key} Value: {x.Value}"));

        /// <summary>
        ///     Creates RabbitMQ message consumption event.
        /// </summary>
        protected ConsumptionEventBase(BasicDeliverEventArgs basicEvent, Dictionary<string, object> data)
        {
            BasicEvent = basicEvent;
            Data = data ?? new();
        }
    }
}