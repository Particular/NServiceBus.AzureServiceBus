namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Addressing
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using AzureServiceBus;
    using Microsoft.ServiceBus;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;
    using NUnit.Framework;
    using Transport.AzureServiceBus;

    public class When_sending_message_with_custom_partitioning_strategy : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_fail_over_to_available_namespace()
        {
            var namespaceManager1 = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(targetConnectionString));
            await namespaceManager1.DeleteQueue(Conventions.EndpointNamingConvention(typeof(TargetEndpoint)));

            var contextWithOnlyNamespace1Available = await Scenario.Define<Context>()
                .WithEndpoint<SourceEndpoint>(b =>
                {
                    b.When(async (bus, c) =>
                    {
                        await bus.Send(new MyRequest());
                        await bus.Send(new MyRequest());
                        await bus.Send(new MyRequest());
                    });
                })
                .WithEndpoint<TargetEndpoint>(b => b.CustomConfig(c => c.ConfigureAzureServiceBus().ConnectionString(connectionString)))
                .Done(c => c.RequestsReceived == 3 && c.NamespaceNames.Count == 3)
                .Run();

            var namespaceManager2 = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(connectionString));
            await namespaceManager2.DeleteQueue(Conventions.EndpointNamingConvention(typeof(TargetEndpoint)));

            var contextWithOnlyNamespace2Available = await Scenario.Define<Context>()
                .WithEndpoint<SourceEndpoint>(b =>
                {
                    b.When(async (bus, c) =>
                    {
                        await bus.Send(new MyRequest());
                        await bus.Send(new MyRequest());
                        await bus.Send(new MyRequest());
                    });
                })
                .WithEndpoint<TargetEndpoint>(b => b.CustomConfig(c => c.ConfigureAzureServiceBus().ConnectionString(targetConnectionString)))
                .Done(c => c.RequestsReceived == 3 && c.NamespaceNames.Count == 3)
                .Run();

            CollectionAssert.AreEquivalent(new[]
            {
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace1",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace1",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace1",
            }, contextWithOnlyNamespace1Available.NamespaceNames);
            CollectionAssert.AreEquivalent(new[]
            {
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace2",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace2",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace2",
            }, contextWithOnlyNamespace2Available.NamespaceNames);
        }

        [Test]
        public async Task Should_round_robin()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<SourceEndpoint>(b =>
                {
                    b.When(async (bus, c) =>
                    {
                        await bus.Send(new MyRequest());
                        await bus.Send(new MyRequest());
                        await bus.Send(new MyRequest());
                        await bus.Send(new MyRequest());
                    });
                })
                .WithEndpoint<TargetEndpoint>(b => b.CustomConfig(c => c.ConfigureAzureServiceBus().ConnectionString(connectionString)))
                .WithEndpoint<TargetEndpoint>(b => b.CustomConfig(c => c.ConfigureAzureServiceBus().ConnectionString(targetConnectionString)))
                .Done(c => c.RequestsReceived == 4 && c.NamespaceNames.Count == 4)
                .Run();

            CollectionAssert.AreEquivalent(new[]
            {
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace1",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace2",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace1",
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