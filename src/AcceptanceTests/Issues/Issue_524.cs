namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Issues
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;
    using NUnit.Framework;

    public class Issue_524 : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task When_publisher_and_subscriber_bundles_are_miscofigured_Should_not_loose_messages()
        {
            var topology = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBusTransport.Topology");
            if (topology != "ForwardingTopology")
            {
                Assert.Inconclusive("The test is designed for ForwardingTopology only.");
            }

            await MimicAnExistingEnvironmentWithAlreadyMisconfiguredBundlesPreCreated();

            await Scenario.Define<Context>()
                .WithEndpoint<Publisher>(builder => builder.When(ctx => ctx.SubscribedToEvent, async session =>
                {
                    var options = new SendOptions();
                    options.RequireImmediateDispatch();
                    options.SetDestination("bundle-3");
                    options.DoNotEnforceBestPractices();
                    await session.Send(new MyEvent(), options);
                }))
                .WithEndpoint<Subscriber>(builder => builder.When((session, ctx) =>
                {
                    ctx.SubscribedToEvent = ctx.HasNativePubSubSupport;
                    return Task.FromResult(0);
                }))
                .Done(ctx => ctx.EventWasHandled)
                .Run();
        }

        static async Task MimicAnExistingEnvironmentWithAlreadyMisconfiguredBundlesPreCreated()
        {
            var connectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            async Task createTopic(string name)
            {
                if (! await namespaceManager.TopicExistsAsync(name))
                {
                    await namespaceManager.CreateTopicAsync(new TopicDescription(name));
                }
            }

            await createTopic("bundle-1");
            await createTopic("bundle-2");
            await createTopic("bundle-3");
        }

        public class Context : ScenarioContext
        {
            public bool SubscribedToEvent { get; set; }
            public bool EventWasHandled { get; set; }
        }

        public class MyEvent : IEvent
        {
        }

        public class Publisher : EndpointConfigurationBuilder
        {
            public Publisher()
            {
                EndpointSetup<DefaultServer>(endpointConfiguration =>
                {
                    endpointConfiguration.Recoverability().DisableLegacyRetriesSatellite();
                    var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
#pragma warning disable 618
                    var topology = transport.UseTopology<ForwardingTopology>();
                    topology.NumberOfEntitiesInBundle(3); // override the default to cause misconfiguration
#pragma warning restore 618
                });
            }
        }

        public class Subscriber : EndpointConfigurationBuilder
        {
            public Subscriber()
            {
                EndpointSetup<DefaultServer>(endpointConfiguration =>
                {
                    endpointConfiguration.Recoverability().DisableLegacyRetriesSatellite();
                });
            }

            public class Handler : IHandleMessages<MyEvent>
            {
                public Context Context { get; set; }

                public Task Handle(MyEvent message, IMessageHandlerContext context)
                {
                    Context.EventWasHandled = true;
                    return Task.FromResult(0);
                }
            }
        }
    }
}