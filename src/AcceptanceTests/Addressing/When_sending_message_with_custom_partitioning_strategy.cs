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

    public class When_sending_message_with_custom_partitioning_strategy : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_honor_strategy()
        {
            // It is not possible to late start an endpoint therefore the switch over to namespace1 cannot be tested
            // Testing that scenario is only possible with pub/sub because there it is possible to subscribe at a later point in time

            var context = await Scenario.Define<Context>()
                .WithEndpoint<SourceEndpoint>(b =>
                {
                    b.When(async (bus, c) =>
                    {
                        await bus.Send(new MyRequest());
                        await bus.Send(new MyRequest());
                        await bus.Send(new MyRequest());
                    });
                })
                .WithEndpoint<TargetEndpoint>(b =>
                {
                    b.CustomConfig(c =>
                    {
                        c.ConfigureAzureServiceBus().ConnectionString(targetConnectionString);
                    });
                })
                .Done(c => c.RequestsReceived == 3 && c.NamespaceNames.Count == 3)
                .Run();

            CollectionAssert.AreEquivalent(new[]
            {
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace2",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace2",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace2",
            }, context.NamespaceNames);
        }

        static string connectionString = connectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");
        static string targetConnectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBus.ConnectionString.Fallback");

        public class SourceEndpoint : EndpointConfigurationBuilder
        {
            public SourceEndpoint()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    var transport = c.ConfigureAzureServiceBus();

                    var partitioning = transport.NamespacePartitioning();
                    partitioning.UseStrategy<RoundRobinPartitioningFailoverStrategy>();
                    partitioning.AddNamespace("namespace1", connectionString);
                    partitioning.AddNamespace("namespace2", targetConnectionString);

                    c.ConfigureTransport().Routing().RouteToEndpoint(typeof(MyRequest), typeof(TargetEndpoint));
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
                EndpointSetup<DefaultServer>();
            }

            class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Context Context { get; set; }

                public async Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    await context.Reply(new MyResponse());
                    Context.Received();
                }
            }
        }

        public class MyRequest : IMessage
        {
        }

        public class MyResponse : IMessage
        {
        }

        class Context : ScenarioContext
        {
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

            long received;
        }
    }
}