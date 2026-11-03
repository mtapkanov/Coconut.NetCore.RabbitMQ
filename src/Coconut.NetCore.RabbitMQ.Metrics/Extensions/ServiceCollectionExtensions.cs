using Coconut.NetCore.RabbitMQ.Core.Extensions;
using Coconut.NetCore.RabbitMQ.Metrics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Coconut.NetCore.RabbitMQ.Metrics.Extensions;

/// <summary>
///     DI registration for RabbitMQ metrics.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Count acknowledged and rejected messages in <c>app_rmq_messages_count_total</c>,
    ///     labeled by <c>{exchange}.{routingKey}</c> and result.
    ///     Exposing the <c>/metrics</c> endpoint is up to the application (e.g. prometheus-net.AspNetCore).
    /// </summary>
    public static IServiceCollection AddRabbitMqMetrics(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<NumberOfProcessedRabbitMqMetrics>();
        return services.AddRabbitMqEventHandler<MetricsRabbitMqEventHandler>();
    }

    /// <summary>
    ///     Set the global <c>app</c> static label on the default Prometheus registry.
    ///     Must be called before any metric is created, prometheus-net rejects static labels on a registry in use.
    /// </summary>
    public static IServiceCollection AddMetricsAppName(this IServiceCollection services, string applicationName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrEmpty(applicationName);

        Prometheus.Metrics.DefaultRegistry.SetStaticLabels(new Dictionary<string, string> { ["app"] = applicationName });
        return services;
    }
}
