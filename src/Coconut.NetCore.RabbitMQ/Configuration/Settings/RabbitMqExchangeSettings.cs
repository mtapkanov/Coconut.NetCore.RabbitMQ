namespace Coconut.NetCore.RabbitMQ.Configuration.Settings
{
    /// <summary>
    ///     RabbitMQ exchange configuration.
    /// </summary>

    public class RabbitMqExchangeSettings
    {
        /// <summary>
        ///     Exchange name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Exchange declaration configuration.
        /// </summary>
        public RabbitMqExchangeDeclareSettings Declare { get; set; }
    }
}
