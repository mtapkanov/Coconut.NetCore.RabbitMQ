namespace Coconut.NetCore.RabbitMQ.Processing.Internal
{
    /// <summary>
    ///     Consume context allow access to details surrounding the inbound message.
    /// </summary>
    /// <typeparam name="TMessage">The type representing messages in the queue.</typeparam>
    internal interface IConsumeContext<out TMessage> : IConsumeContext
    {
        /// <summary>
        ///     Represents message received from queue.
        /// </summary>
        TMessage Message { get; }
    }
}