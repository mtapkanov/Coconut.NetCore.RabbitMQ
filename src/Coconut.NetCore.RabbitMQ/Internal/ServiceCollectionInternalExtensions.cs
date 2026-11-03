using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Coconut.NetCore.RabbitMQ.Internal;

internal static class ServiceCollectionInternalExtensions
{
    public static void TryAddSingletonSelf<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IServiceCollection services)
        where T : class =>
        services.TryAddSingleton<T>();

    public static void TryAddSelf<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IServiceCollection services, ServiceLifetime lifetime)
        where T : class =>
        services.TryAdd(ServiceDescriptor.Describe(typeof(T), typeof(T), lifetime));
}
