using System.Collections.Generic;
using Coconut.NetCore.RabbitMQ.Processing.Internal;
using RabbitMQ.Client.Events;

namespace Coconut.NetCore.RabbitMQ.Processing
{
    /// <inheritdoc />
    public class ConsumeContext<TMessage> : IConsumeContext<TMessage>
    {
        /// <inheritdoc />
        public TMessage Message { get; }

        /// <inheritdoc />
        public BasicDeliverEventArgs BasicEvent { get; }

        /// <inheritdoc />
        public bool Failed { get; set; }

        /// <inheritdoc />
        public Dictionary<string, object> Data { get; } = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="basicEvent"></param>
        /// <param name="message"></param>
        public ConsumeContext(BasicDeliverEventArgs basicEvent, TMessage message)
        {
            BasicEvent = basicEvent;
            Message = message;
        }
    }
}
