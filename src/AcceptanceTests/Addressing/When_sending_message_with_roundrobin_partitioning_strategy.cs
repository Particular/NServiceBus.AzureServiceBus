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

    public class When_sending_message_with_roundrobin_partitioning_strategy : NServiceBusAcceptanceTest
    {
        static string connectionString = connectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");
        static string targetConnectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBus.ConnectionString.Fallback");

        [Test]
        public async Task Should_round_robin_between_active_namespaces()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<SourceEndpoint>(b =>
                {
                    b.When(async (bus, c) =>
                    {
                        await bus.Send(new MyRequest());
                        await bus.Send(new MyRequest());
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
      

        public class SourceEndpoint : EndpointConfigurationBuilder
        {
            public SourceEndpoint()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    var transport = c.ConfigureAzureServiceBus();

                    var partitioning = transport.NamespacePartitioning();
                    partitioning.UseStrategy<RoundRobinNamespacePartitioning>();
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

                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    Context.Received();
                    return context.Reply(new MyResponse());
                }
            }
        }

        public class MyRequest : IMessage
        {
        }

        public class MyRequestImpl : MyRequest
        {
        }

        public class MyResponse : IMessage
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