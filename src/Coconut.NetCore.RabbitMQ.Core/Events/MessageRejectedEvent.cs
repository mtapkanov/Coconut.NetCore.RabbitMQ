using System.Collections.Generic;
using Coconut.NetCore.RabbitMQ.Events;
using RabbitMQ.Client.Events;

namespace Coconut.NetCore.RabbitMQ.Core.Events
{
    /// <summary>
    ///     RabbitMQ message rejected event.
    /// </summary>
    public class MessageRejectedEvent : ConsumptionEventBase
    {
        /// <summary>
        ///     Creates RabbitMQ message rejected event.
        /// </summary>
        public MessageRejectedEvent(BasicDeliverEventArgs basicEvent, Dictionary<string, object> data) : base(basicEvent, data) { }
    }
}
