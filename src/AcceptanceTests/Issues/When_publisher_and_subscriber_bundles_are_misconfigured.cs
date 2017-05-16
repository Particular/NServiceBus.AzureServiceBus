﻿namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTest
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;
    using NUnit.Framework;

    public class When_publisher_and_subscriber_bundles_are_misconfigured : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_not_lose_messages()
        {
            var topology = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBusTransport.Topology");
            if (topology != "ForwardingTopology")
            {
                Assert.Inconclusive("The test is designed for ForwardingTopology only.");
            }

            await MimicAnExistingEnvironmentWithAlreadyMisconfiguredBundlesPreCreated();

            await Scenario.Define<Context>()
                .WithEndpoint<Publisher>(builder => builder.When(ctx => ctx.SubscribedToEvent, session =>
                {
                    var options = new SendOptions();
                    options.RequireImmediateDispatch();
                    options.SetDestination("bundle-3");
                    options.DoNotEnforceBestPractices();
                    return session.Send(new MyEvent(), options);
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

            await CreateTopic(namespaceManager, "bundle-1");
            await CreateTopic(namespaceManager, "bundle-2");
            await CreateTopic(namespaceManager, "bundle-3");
        }

        static async Task CreateTopic(NamespaceManager namespaceManager, string name)
        {
            if (name.Equals("bundle-3"))
            {
                await namespaceManager.DeleteTopicAsync(name);
            }

            if (!await namespaceManager.TopicExistsAsync(name))
            {
                await namespaceManager.CreateTopicAsync(new TopicDescription(name));
            }
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