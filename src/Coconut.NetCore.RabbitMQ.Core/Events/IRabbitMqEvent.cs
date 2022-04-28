using System.Collections.Generic;

namespace Coconut.NetCore.RabbitMQ.Core.Events
{
    /// <summary>
    ///     Common interface for RabbitMQ bus events.
    /// </summary>
    public interface IRabbitMqEvent
    {
        /// <summary>
        ///     Provides store for custom event data.
        /// </summary>
        Dictionary<string, object> Data { get; }

        /// <summary>
        ///     Representation of event properties as a logging string.
        /// </summary>
        string ToString();
    }
}