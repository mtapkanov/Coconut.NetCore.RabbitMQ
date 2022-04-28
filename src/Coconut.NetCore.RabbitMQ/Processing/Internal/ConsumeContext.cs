using System.Collections.Generic;
using RabbitMQ.Client.Events;

namespace Coconut.NetCore.RabbitMQ.Processing.Internal
{
    internal class ConsumeContext<TMessage> : IConsumeContext<TMessage>
    {
        public TMessage Message { get; }

        public BasicDeliverEventArgs BasicEvent { get; }

        public bool Failed { get; set; }

        public Dictionary<string, object> Data { get; } = new();

        public ConsumeContext(BasicDeliverEventArgs basicEvent, TMessage message)
        {
            BasicEvent = basicEvent;
            Message = message;
        }
    }
}
