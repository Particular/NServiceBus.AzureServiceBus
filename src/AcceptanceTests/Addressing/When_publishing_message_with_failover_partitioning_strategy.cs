namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Addressing
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using AzureServiceBus;
    using Configuration.AdvancedExtensibility;
    using Microsoft.ServiceBus;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;
    using NUnit.Framework;
    using Transport.AzureServiceBus;

    public class When_publishing_message_with_failover_partitioning_strategy : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_failover_in_case_of_failure()
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
                .WithEndpoint<TargetEndpoint>(b => b.CustomConfig(c => c.ConfigureAzureServiceBus().ConnectionString(targetConnectionString))) // target endpoint only available on failover namespace
                .Done(c => c.RequestsReceived == 2 && c.NamespaceNames.Count == 2)
                .Run();

            CollectionAssert.AreEquivalent(new[]
            {
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace2",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace2"
            }, context.NamespaceNames);
        }

        [TearDown]
        public Task TearDown()
        {
            var namespaceManager1 = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(connectionString));
            var namespaceManager2 = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(targetConnectionString));
            var topic = $"{BundlePrefix}1";
            return Task.WhenAll(namespaceManager1.DeleteTopic(topic), namespaceManager2.DeleteTopic(topic));
        }

        static string connectionString = connectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");
        static string targetConnectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBus.ConnectionString.Fallback");
        static string BundlePrefix = $"bundle{DateTime.UtcNow.Ticks}-";

        public class Publisher : EndpointConfigurationBuilder
        {
            public Publisher()
            {
                EndpointSetup<DefaultPublisher>(c =>
                {
                    var transport = c.ConfigureAzureServiceBus();

                    if (TestSuiteConstraints.Current.IsForwardingTopology)
                    {
                        c.GetSettings().Set(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix, BundlePrefix);
                    }

                    transport.Topics().EnableFilteringMessagesBeforePublishing(true);

                    var partitioning = transport.NamespacePartitioning();
                    partitioning.UseStrategy<FailOverNamespacePartitioning>();
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
                    var transport = c.ConfigureAzureServiceBus();

                    if (TestSuiteConstraints.Current.IsForwardingTopology)
                    {
                        c.GetSettings().Set(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix, BundlePrefix);
                    }

                    transport.Topics().EnableFilteringMessagesBeforePublishing(true);
                }, metadata => { metadata.RegisterPublisherFor<MyEvent>(typeof(Publisher)); });
            }

            class MyRequestHandler : IHandleMessages<MyEvent>
            {
                public Context Context { get; set; }

                public Task Handle(MyEvent message, IMessageHandlerContext context)
                {
                    Context.Received();
                    return context.Reply(new MyResponse());
                }
            }
        }

        public class MyEvent : IEvent
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