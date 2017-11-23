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
    using Features;
    using Microsoft.ServiceBus;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;
    using NUnit.Framework;
    using Transport.AzureServiceBus;

    public class When_publishing_message_with_custom_partitioning_strategy : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_honor_strategy()
        {
            var waitToSubscribe = new TaskCompletionSource<bool>();
            var subscribed = new TaskCompletionSource<bool>();

            var context = await Scenario.Define<Context>()
                .WithEndpoint<Publisher>(b =>
                {
                    b.When(async (bus, c) =>
                    {
                        await bus.Publish(new MyEvent());
                        await bus.Publish(new MyEvent());
                        waitToSubscribe.SetResult(true);
                        await subscribed.Task;
                        await bus.Publish(new MyEvent());
                        await bus.Publish(new MyEvent());
                        await bus.Publish(new MyEvent());
                        await bus.Publish(new MyEvent());
                    });
                })
                .WithEndpoint<TargetEndpoint>(b =>
                {
                    b.When(async (bus, c) =>
                    {
                        await waitToSubscribe.Task;
                        await bus.Subscribe(typeof(MyEvent));
                        subscribed.SetResult(true);
                    });
                    b.CustomConfig(c => { c.ConfigureAzureServiceBus().ConnectionString(connectionString); });
                })
                .WithEndpoint<TargetEndpoint>(b =>
                {
                    b.CustomConfig(c =>
                    {
                        c.ConfigureAzureServiceBus().ConnectionString(targetConnectionString);
                        c.EnableFeature<AutoSubscribe>();
                    });
                })
                .Done(c => c.RequestsReceived == 6 && c.NamespaceNames.Count == 6)
                .Run();

            CollectionAssert.AreEquivalent(new[]
            {
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace2",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace2",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace1",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace2",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace2",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace1"
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
                    partitioning.UseStrategy<RoundRobinPartitioningFailoverStrategy>();
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
                    c.DisableFeature<AutoSubscribe>();

                    var transport = c.ConfigureAzureServiceBus();

                    if (TestSuiteConstraints.Current.IsForwardingTopology)
                    {
                        c.GetSettings().Set(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix, BundlePrefix);
                    }

                    transport.Topics().EnableFilteringMessagesBeforePublishing(true);
                }, metadata => metadata.RegisterPublisherFor<MyEvent>(typeof(Publisher)));
            }

            class MyRequestHandler : IHandleMessages<MyEvent>
            {
                public Context Context { get; set; }

                public async Task Handle(MyEvent message, IMessageHandlerContext context)
                {
                    await context.Reply(new MyResponse());
                    Context.Received();
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