using System.Collections.Generic;
using RabbitMQ.Client.Events;

namespace Coconut.NetCore.RabbitMQ.Processing
{
    /// <summary>
    ///     Consume context allow access to details surrounding the inbound message.
    /// </summary>
    public interface IConsumeContext
    {
        /// <summary>
        ///     Contains all the information about a message delivered from an AMQP broker within
        ///     the Basic content-class.
        /// </summary>
        BasicDeliverEventArgs BasicEvent { get; }
        
        /// <summary>
        ///     If set true, consumption marked failed and message must be returned to queue.
        /// </summary>
        bool Failed { get; set; }
        
        /// <summary>
        ///     Stores user data that will be passed to events.
        /// </summary>
        Dictionary<string, object> Data { get; }
    }
}
