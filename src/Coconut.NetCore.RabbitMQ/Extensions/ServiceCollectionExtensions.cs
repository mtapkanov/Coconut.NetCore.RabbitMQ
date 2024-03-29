﻿using System;
using System.Collections.Generic;
using Coconut.NetCore.RabbitMQ.Configuration.Builders;
using Coconut.NetCore.RabbitMQ.Configuration.Options;
using Coconut.NetCore.RabbitMQ.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Coconut.NetCore.RabbitMQ.Extensions
{
    /// <summary>
    ///     RabbitMQ service collection extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds RabbitMQ definition to service collection.
        /// </summary>
        /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
        /// <param name="uri">RabbitMQ URI.</param>
        /// <param name="optionsAction">Configuring parameters.</param>
        /// <returns>Contract for a collection of service descriptors where the RabbitMqBus registered.</returns>
        public static IServiceCollection AddRabbitMq(this IServiceCollection services, Uri uri, Action<RabbitMqOptionsBuilder> optionsAction)
        {
            services.AddHostedService<RabbitMqStarterHostedService>();
            services.AddSingleton<IRabbitMqStarter, RabbitMqStarter>();
            services.TryAddSingleton<RabbitMqEventBus>();
            services.TryAddSingleton<PublisherCache>();
            services.TryAddSingleton<IRabbitMqBus, RabbitMqBus>();
            services.AddTransient<RabbitMqUnit>();

            var options = new RabbitMqOptionsBuilder(services, uri);
            optionsAction(options);
            services.AddTransient(_ => ((IBuilder<RabbitMqOptions>)options).Build());

            return services;
        }
    }
}
