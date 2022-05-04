using Coconut.NetCore.RabbitMQ.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Coconut.NetCore.RabbitMQ.Core.Extensions
{
    /// <summary>
    ///     RabbitMQ service collection extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds RabbitMQ message processing event handler to service collection.
        /// </summary>
        /// <typeparam name="TRabbitMqEventHandler">Type of handler</typeparam>
        /// <param name="services">Specifies the contract for a collection of service descriptors</param>
        /// <returns>Contract for a collection of service descriptors where the RabbitMqBus registered</returns>
        public static IServiceCollection AddRabbitMqEventHandler<TRabbitMqEventHandler>(this IServiceCollection services)
            where TRabbitMqEventHandler : class, IRabbitMqEventHandler
        {
            services.AddSingleton<IRabbitMqEventHandler, TRabbitMqEventHandler>();
            return services;
        }
    }
}