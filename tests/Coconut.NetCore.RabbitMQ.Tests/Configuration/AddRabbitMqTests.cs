using System.Text;
using Coconut.NetCore.RabbitMQ.Configuration.Settings;
using Coconut.NetCore.RabbitMQ.Extensions;
using Coconut.NetCore.RabbitMQ.Internal;
using Coconut.NetCore.RabbitMQ.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Coconut.NetCore.RabbitMQ.Tests.Configuration;

public sealed class AddRabbitMqTests
{
    private static readonly Uri Uri = new("amqp://guest:guest@localhost:5672/");

    private sealed class Serializer : IMessageSerializer<string>, IMessageDeserializer<string>
    {
        public ReadOnlyMemory<byte> Serialize(string message) => Encoding.UTF8.GetBytes(message);

        public string Deserialize(ReadOnlySpan<byte> body) => Encoding.UTF8.GetString(body);
    }

    private sealed class Consumer : IMessageConsumer<string>
    {
        public Task ConsumeAsync(ConsumeContext<string> context, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    [Fact]
    public void ExchangeWithoutMessages_ThrowsAtRegistration()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddRabbitMq(Uri, o => o.AddExchange(new RabbitMqExchangeSettings { Name = "ex" })));

        Assert.Contains("ex", exception.Message);
    }

    [Fact]
    public void QueueWithoutConsumer_ThrowsAtRegistration() =>
        Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddRabbitMq(Uri, o => o.AddQueue<string>(new RabbitMqQueueSettings { Name = "q" }).UseDeserializer<Serializer>()));

    [Fact]
    public void QueueWithoutDeserializer_ThrowsAtRegistration() =>
        Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddRabbitMq(Uri, o => o.AddQueue<string>(new RabbitMqQueueSettings { Name = "q" }).UseConsumer<Consumer>()));

    [Fact]
    public void ZeroPrefetch_ThrowsAtRegistration() =>
        Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddRabbitMq(Uri, o => o.AddQueue<string>(new RabbitMqQueueSettings { Name = "q", PrefetchCount = 0 })
                .UseDeserializer<Serializer>()
                .UseConsumer<Consumer>()));

    [Fact]
    public void ValidConfiguration_RegistersSharedInfrastructureOnce_AndOneDefinitionPerConnection()
    {
        var services = new ServiceCollection();

        for (var i = 0; i < 2; i++)
            services.AddRabbitMq(Uri, o =>
            {
                o.AddExchange(new RabbitMqExchangeSettings { Name = "ex" }).AcceptMessages<string, Serializer>();
                o.AddQueue<string>(new RabbitMqQueueSettings { Name = "q" }).UseDeserializer<Serializer>().UseConsumer<Consumer>();
            });

        Assert.Single(services, d => d.ServiceType == typeof(IHostedService));
        Assert.Single(services, d => d.ServiceType == typeof(IRabbitMqBus));
        Assert.Equal(2, services.Count(d => d.ServiceType == typeof(ConnectionDefinition)));
        Assert.Equal(ServiceLifetime.Singleton, Assert.Single(services, d => d.ServiceType == typeof(Serializer)).Lifetime);
        Assert.Equal(ServiceLifetime.Scoped, Assert.Single(services, d => d.ServiceType == typeof(Consumer)).Lifetime);
    }
}
