using System.Collections.Generic;
using Coconut.NetCore.RabbitMQ.Events;
using RabbitMQ.Client.Events;

namespace Coconut.NetCore.RabbitMQ.Core.Events
{
    /// <summary>
    ///     RabbitMQ message acknowledged event.
    /// </summary>
    public class MessageAcknowledgedEvent : ConsumptionEventBase
    {
        /// <summary>
        ///     Creates RabbitMQ message acknowledged event.
        /// </summary>
        public MessageAcknowledgedEvent(BasicDeliverEventArgs basicEvent, Dictionary<string, object> data) : base(basicEvent, data) { }

    }
}
