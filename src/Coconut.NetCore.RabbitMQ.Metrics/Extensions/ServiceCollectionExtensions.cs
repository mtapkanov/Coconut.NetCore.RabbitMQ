using System;
using System.Collections.Generic;
using Coconut.NetCore.RabbitMQ.Core.Extensions;
using Coconut.NetCore.RabbitMQ.Metrics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Coconut.NetCore.RabbitMQ.Metrics.Extensions
{
    /// <summary>
    ///     RabbitMQ metrics service collection extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds RabbitMQ basic metrics.
        /// </summary>
        /// <param name="services">Specifies the contract for a collection of service descriptors</param>
        /// <returns>Contract for a collection of service descriptors where the RabbitMqBus registered</returns>
        public static IServiceCollection AddRabbitMqMetrics(this IServiceCollection services)
        {
            services.TryAddSingleton<NumberOfProcessedRabbitMqMetrics>();
            return services.AddRabbitMqEventHandler<MetricsRabbitMqEventHandler>();
        }

        /// <summary>
        ///     Adds label with name 'app' to all metrics. The label contains application name.
        /// </summary>
        /// <param name="services">Specifies the contract for a collection of service descriptors</param>
        /// <param name="applicationName">Application name</param>
        public static IServiceCollection AddMetricsAppName(this IServiceCollection services, string applicationName)
        {
            if (string.IsNullOrEmpty(applicationName))
                throw new ArgumentNullException(nameof(applicationName));

            Prometheus.Metrics.DefaultRegistry.SetStaticLabels(new Dictionary<string, string>
            {
                ["app"] = applicationName
            });

            return services;
        }
    }
}