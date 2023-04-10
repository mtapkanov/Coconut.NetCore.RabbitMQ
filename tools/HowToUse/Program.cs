using Coconut.NetCore.RabbitMQ.Extensions;
using Coconut.NetCore.RabbitMQ.Metrics.Extensions;
using HowToUse;
using HowToUse.Application;
using HowToUse.Infrastructure.Configuration;
using HowToUse.Infrastructure.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();
builder.Services.UseConfiguration(builder.Configuration);

var rabbitMqSettings = AppConfiguration.RabbitMqSettings;

builder.Services.AddRabbitMq(rabbitMqSettings!.BaseUri, options =>
{
    options.AddExchange(rabbitMqSettings.Exchange)
        .AcceptMessages<Message, MessageSerializer>(_ => "*");

    options.AddQueue<Message>(rabbitMqSettings.Queue)
        .UseConsumer<MessageConsumer>()
        .UseDeserializer<MessageDeserializer>();
});

builder.Services.AddHostedService<InfinitePushMessagesToRabbitMqHostedService>();

builder.Services.AddMetricsAppName(nameof(HowToUse))
    .AddRabbitMqMetrics();

var app = builder.Build();
app.Run();