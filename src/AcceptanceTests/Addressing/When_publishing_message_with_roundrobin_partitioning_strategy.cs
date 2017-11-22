namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Addressing
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using AzureServiceBus;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;
    using NUnit.Framework;

    public class When_publishing_message_with_roundrobin_partitioning_strategy : NServiceBusAcceptanceTest
    {
        static string connectionString = connectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");
        static string targetConnectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBus.ConnectionString.Fallback");

        [Test]
        public async Task Should_round_robin_between_active_namespaces()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<Publisher>(b =>
                {
                    b.When(async (bus, c) =>
                    {
                        await bus.Publish(new MyEvent());
                        await bus.Publish(new MyEvent());
                    });
                })
                .WithEndpoint<TargetEndpoint>(b => b.CustomConfig(c => c.ConfigureAzureServiceBus().ConnectionString(connectionString)))
                .WithEndpoint<TargetEndpoint>(b => b.CustomConfig(c => c.ConfigureAzureServiceBus().ConnectionString(targetConnectionString)))
                .Done(c => c.RequestsReceived == 2 && c.NamespaceNames.Count == 2)
                .Run();

            CollectionAssert.AreEquivalent(new[]
            {
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace2",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace1",
            }, context.NamespaceNames);
        }
      

        public class Publisher : EndpointConfigurationBuilder
        {
            public Publisher()
            {
                EndpointSetup<DefaultPublisher>(c =>
                {
                    var transport = c.ConfigureAzureServiceBus();

                    var partitioning = transport.NamespacePartitioning();
                    partitioning.UseStrategy<RoundRobinNamespacePartitioning>();
                    partitioning.AddNamespace("namespace1", connectionString);
                    partitioning.AddNamespace("namespace2", targetConnectionString);
                });
            }

            class MyResponseHandler : IHandleMessages<MyResponse>
            {
                public Context Context { get; set; }

                public Task Handle(MyResponse message, IMessageHandlerContext context)
                {
                    Context.NamespaceNames.Add(context.ReplyToAddress);
                    return TaskEx.Completed;
                }
            }
        }

        public class TargetEndpoint : EndpointConfigurationBuilder
        {
            public TargetEndpoint()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    c.ConfigureTransport().Routing().RouteToEndpoint(typeof(MyResponse), typeof(Publisher));
                });
            }

            class MyRequestHandler : IHandleMessages<MyEvent>
            {
                public Context Context { get; set; }

                public async Task Handle(MyEvent message, IMessageHandlerContext context)
                {
                    await context.Send(new MyResponse());
                    Context.Received();
                }
            }
        }

        public class MyEvent : IEvent
        {
        }

        public class MyResponse : ICommand
        {
        }

        class Context : ScenarioContext
        {
            long received;
            public Context()
            {
                NamespaceNames = new ConcurrentBag<string>();
            }

            public long RequestsReceived => Interlocked.Read(ref received);

            public ConcurrentBag<string> NamespaceNames { get; }

            public void Received()
            {
                Interlocked.Increment(ref received);
            }
        }
    }
}