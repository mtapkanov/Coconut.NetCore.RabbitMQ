using System;
using System.Collections.Generic;
using System.Threading;
using AutoFixture;
using Coconut.NetCore.RabbitMQ.Configuration;
using Coconut.NetCore.RabbitMQ.Configuration.Settings;
using Coconut.NetCore.RabbitMQ.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Coconut.NetCore.RabbitMQ.Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("App started");
            
            var rabbitMqUri = new Uri("amqp://guest:guest@localhost:5672/test");
            var rabbitMqExchangeSettings = new RabbitMqExchangeSettings
            {
                Name = "exchange",
                Declare = new RabbitMqExchangeDeclareSettings
                {
                    Durable = true,
                    Type = ExchangeType.Direct
                }
            };
            
            var rabbitMqQueueSettings = new RabbitMqQueueSettings
            {
                Name = "queue",
                PrefetchCount = 16,
                FailDelay = TimeSpan.FromSeconds(5),
                Declare = new RabbitMqQueueDeclareSettings
                {
                    Durable = true,
                    Bindings = new List<RabbitMqQueueBindingSettings>
                    {
                        new RabbitMqQueueBindingSettings
                        {
                            Exchange = "exchange",
                            RoutingKey = "*"
                        }
                    }
                }
            };

            var services = new ServiceCollection();

            services.AddLogging();
            services.AddRabbitMq(rabbitMqUri, builder =>
            {
                builder.AddExchange(rabbitMqExchangeSettings)
                    .AcceptMessages<Message, MessageSerializer>(_ => "*");

                builder.AddQueue<Message>(rabbitMqQueueSettings)
                    .UseConsumer<MessageConsumer>()
                    .UseDeserializer<MessageDeserializer>();
            });

            var provider = services.BuildServiceProvider();

            var requiredService = provider.GetRequiredService<IHostedService>();
            requiredService.StartAsync(CancellationToken.None);

            while (true)
            {
                Console.WriteLine("Press any key what publish messages...");
                
                var consoleKeyInfo = Console.ReadKey();

                if (consoleKeyInfo.Key == ConsoleKey.Enter)
                {
                    var bus = provider.GetRequiredService<IRabbitMqBus>();

                    var fixture = new Fixture();
                    foreach (var message in fixture.CreateMany<Message>(10_000))
                    {
                        bus.Publish(message);
                    }
                }
            }
        }
    }

    [Serializable]
    public class Message
    {
        public int Id { get; set; }
        public string Foo { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Foo)}: {Foo}";
        }
    }
}