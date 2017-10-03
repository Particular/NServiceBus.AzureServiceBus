namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using Microsoft.ServiceBus;
    using NUnit.Framework;
    using NServiceBus.AcceptanceTests;
    using AcceptanceTesting.Customization;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;

    public class When_unsubscribing : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_remove_subscription_from_topology()
        {
            await Scenario.Define<ScenarioContext>()
                .WithEndpoint<Endpoint>(b => b.When((session, c) => session.Unsubscribe<MyEvent>()))
                .Done(c => c.EndpointsStarted)
                .Run();

            var connectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            var endpointName = Conventions.EndpointNamingConvention(typeof(Endpoint));

            if (TestSuiteConstraints.Current.IsForwardingTopology)
            {
                var isSubscriptionFound = await namespaceManager.SubscriptionExistsAsync("bundle-1", endpointName);
                Assert.IsFalse(isSubscriptionFound, "Subscription under 'bundle-1' should have been deleted, but it wasn't.");

                isSubscriptionFound = await namespaceManager.SubscriptionExistsAsync("bundle-2", endpointName);
                Assert.IsFalse(isSubscriptionFound, "Subscription under 'bundle-2' should have been deleted, but it wasn't.");
            }
            else // EndpointOrientedTopology
            {
                var isSubscriptionFound = await namespaceManager.SubscriptionExistsAsync($"{endpointName}.events", $"{endpointName}.{nameof(MyEvent)}");
                Assert.IsFalse(isSubscriptionFound, "Subscription should have been deleted, but it wasn't.");
            }
        }

        public class Endpoint : EndpointConfigurationBuilder
        {
            public Endpoint()
            {
                EndpointSetup<DefaultServer>(c => c.ConfigureTransport().Routing().RouteToEndpoint(typeof(MyEvent), typeof(Endpoint)));
            }

            public class Handler : IHandleMessages<MyEvent>
            {
                public Task Handle(MyEvent message, IMessageHandlerContext context)
                {
                    return Task.FromResult(0);
                }
            }
        }

        public class MyEvent : IEvent
        {
        }
    }
}