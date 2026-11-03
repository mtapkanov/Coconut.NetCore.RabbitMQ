using Coconut.NetCore.RabbitMQ.Extensions;
using Coconut.NetCore.RabbitMQ.Metrics.Extensions;
using HowToUse;
using HowToUse.Application;
using HowToUse.Infrastructure.Configuration;
using HowToUse.Infrastructure.Serialization;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

var rabbitMq = builder.Configuration.GetRequiredSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>()
    ?? throw new InvalidOperationException($"Section '{RabbitMqSettings.SectionName}' is empty.");

builder.Services
    .AddMetricsAppName("HowToUse")
    .AddRabbitMqMetrics()
    .AddRabbitMq(rabbitMq.BaseUri, options =>
    {
        options.AddExchange(rabbitMq.Exchange)
            .AcceptMessages<Message, MessageJsonSerializer>(
                routingKey: static _ => MessageProducer.RoutingKey,
                configureProperties: static (message, properties) => properties.CorrelationId = message.Id.ToString());

        options.AddQueue<Message>(rabbitMq.Queue)
            .UseDeserializer<MessageJsonSerializer>()
            .UseConsumer<MessageConsumer>();
    })
    // Registered after AddRabbitMq: hosted services start in registration order, the bus is connected by then.
    .AddHostedService<MessageProducer>();

var app = builder.Build();

app.MapMetrics();

app.Run();
