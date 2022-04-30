namespace Coconut.NetCore.RabbitMQ.Configuration.Builders
{
    /// <summary>
    ///     Represents configuration builder for options.
    /// </summary>
    /// <typeparam name="TOptions">The type of options to be built.</typeparam>
    public interface IBuilder<TOptions> where TOptions : class
    {
        /// <summary>
        ///     Builds options.
        /// </summary>
        TOptions Build();
    }
}
