namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Addressing
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using AzureServiceBus;
    using AzureServiceBus.AcceptanceTests.Infrastructure;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_publishing_message_with_default_partitioning_strategy : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_send_to_one_namespace_only()
        {
            if (TestSuiteConstraints.Current.IsEndpointOrientedTopology)
            {
                var topicEndpointOrientedTopology = $"{Conventions.EndpointNamingConvention(typeof(Publisher))}.events";

                var namespaceManager2 = NamespaceManager.CreateFromConnectionString(targetConnectionString);
                await namespaceManager2.CreateTopicAsync(new TopicDescription(topicEndpointOrientedTopology));
            }

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
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace1",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace1"
            }, context.NamespaceNames);
        }

        [TearDown]
        public Task TearDown()
        {
            var topicEndpointOrientedTopology = $"{Conventions.EndpointNamingConvention(typeof(Publisher))}.events";

            var namespaceManager2 = NamespaceManager.CreateFromConnectionString(targetConnectionString);
            return namespaceManager2.DeleteTopicAsync(topicEndpointOrientedTopology);
        }

        static string connectionString = connectionString = TestUtility.DefaultConnectionString;
        static string targetConnectionString = TestUtility.FallbackConnectionString;

        public class Publisher : EndpointConfigurationBuilder
        {
            public Publisher()
            {
                EndpointSetup<DefaultPublisher>(c =>
                {
                    var transport = c.ConfigureAzureServiceBus();
                    var partitioning = transport.NamespacePartitioning();
                    partitioning.UseStrategy<SingleNamespacePartitioning>();
                    partitioning.AddNamespace("namespace1", connectionString);
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
                EndpointSetup<DefaultServer>(
                    c => c.ConfigureTransport().Routing().RouteToEndpoint(typeof(MyResponse), typeof(Publisher)),
                    metadata => metadata.RegisterPublisherFor<MyEvent>(typeof(Publisher)));
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
            public Context()
            {
                NamespaceNames = new ConcurrentBag<string>();
            }

            public long RequestsReceived => Interlocked.Read(ref received);

            public ConcurrentBag<string> NamespaceNames { get; }

            public void Received() => Interlocked.Increment(ref received);

            long received;
        }
    }
}