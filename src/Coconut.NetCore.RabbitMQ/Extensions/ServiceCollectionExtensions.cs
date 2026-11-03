using Coconut.NetCore.RabbitMQ.Configuration.Builders;
using Coconut.NetCore.RabbitMQ.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Coconut.NetCore.RabbitMQ.Extensions;

/// <summary>
///     DI registration of the RabbitMQ bus.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers a broker connection with its exchanges and queues, <see cref="IRabbitMqBus" /> and the hosted service
    ///     that connects on host start. Call it several times to work with several brokers or vhosts.
    ///     Configuration is validated here: an invalid builder setup throws immediately.
    /// </summary>
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, Uri uri, Action<RabbitMqOptionsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(configure);

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<PublisherRegistry>();
        services.TryAddSingleton<RabbitMqEventBus>();
        services.TryAddSingleton<IRabbitMqBus, RabbitMqBus>();
        services.AddHostedService<RabbitMqHostedService>();

        var builder = new RabbitMqOptionsBuilder(services, uri);
        configure(builder);
        services.AddSingleton(builder.Build());

        return services;
    }
}
