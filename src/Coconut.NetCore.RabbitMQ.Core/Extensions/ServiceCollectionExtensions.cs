using System.Diagnostics.CodeAnalysis;
using Coconut.NetCore.RabbitMQ.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Coconut.NetCore.RabbitMQ.Core.Extensions;

/// <summary>
///     DI registration for bus event handlers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers a singleton event handler. Registering the same handler type twice has no effect.
    /// </summary>
    public static IServiceCollection AddRabbitMqEventHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRabbitMqEventHandler>(this IServiceCollection services)
        where TRabbitMqEventHandler : class, IRabbitMqEventHandler
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IRabbitMqEventHandler, TRabbitMqEventHandler>());
        return services;
    }
}
