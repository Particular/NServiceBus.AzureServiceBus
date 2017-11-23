namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using Microsoft.ServiceBus;
    using NUnit.Framework;
    using NServiceBus.AcceptanceTests;
    using AcceptanceTesting.Customization;
    using AzureServiceBus.AcceptanceTests.Infrastructure;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AcceptanceTests.EndpointTemplates;

    public class When_unsubscribing_from_one_of_the_events_for_ForwardingTopology : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_remove_subscription_rule_from_topology()
        {
            Requires.ForwardingTopology();

            var connectionString = TestUtility.DefaultConnectionString;
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            var rawEndpointName = Conventions.EndpointNamingConvention(typeof(Endpoint));
            var endpointName = MD5HashBuilder.Build(rawEndpointName);
            var sanitizedEventFullName = typeof(MyOtherEvent).FullName.Replace("+", string.Empty);
            var otherRuleName = MD5HashBuilder.Build(sanitizedEventFullName);

            await CreateSecondTopicInBundleForBackwardsCompatibilityTesting(namespaceManager, endpointName, connectionString, otherRuleName);

            await Scenario.Define<ScenarioContext>()
                .WithEndpoint<Endpoint>(b => b.When((session, c) => session.Unsubscribe<MyEvent>()))
                .Done(c => c.EndpointsStarted)
                .Run();

            var isSubscriptionFound = await namespaceManager.SubscriptionExistsAsync("bundle-1", endpointName);
            Assert.IsTrue(isSubscriptionFound, "Subscription under 'bundle-1' should have been found, but it wasn't.");

            isSubscriptionFound = await namespaceManager.SubscriptionExistsAsync("bundle-2", endpointName);
            Assert.IsTrue(isSubscriptionFound, "Subscription under 'bundle-2' should have been found, but it wasn't.");

            var rules = await namespaceManager.GetRulesAsync("bundle-1", endpointName);
            CollectionAssert.AreEquivalent(new[] { otherRuleName }, rules.Select(r => r.Name));

            rules = await namespaceManager.GetRulesAsync("bundle-2", endpointName);
            CollectionAssert.AreEquivalent(new[] { otherRuleName }, rules.Select(r => r.Name));
        }

        static async Task CreateSecondTopicInBundleForBackwardsCompatibilityTesting(NamespaceManager namespaceManager, string endpointName, string connectionString, string ruleForMyOtherEvent)
        {
            const string topicPath = "bundle-2";
            if (!await namespaceManager.TopicExistsAsync(topicPath))
            {
                await namespaceManager.CreateTopicAsync(topicPath);
            }

            var subscriptionClient = SubscriptionClient.CreateFromConnectionString(connectionString, topicPath, endpointName);
            Task CreateRuleForMyEvent() => subscriptionClient.AddRuleAsync(MD5HashBuilder.Build(typeof(MyEvent).FullName.Replace("+", string.Empty)), new TrueFilter());

            if (!await namespaceManager.SubscriptionExistsAsync(topicPath, endpointName))
            {
                await namespaceManager.CreateSubscriptionAsync(topicPath, endpointName);
                await subscriptionClient.RemoveRuleAsync(RuleDescription.DefaultRuleName);
                await subscriptionClient.AddRuleAsync(ruleForMyOtherEvent, new TrueFilter());
                await CreateRuleForMyEvent();
            }
            else
            {
                await CreateRuleForMyEvent();
            }
        }

        public class Endpoint : EndpointConfigurationBuilder
        {
            public Endpoint()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    c.ConfigureTransport().Routing().RouteToEndpoint(typeof(MyEvent), typeof(Endpoint));
                    c.ConfigureTransport().Routing().RouteToEndpoint(typeof(MyOtherEvent), typeof(Endpoint));
                });
            }

            public class Handler : IHandleMessages<MyEvent>, IHandleMessages<MyOtherEvent>
            {
                public Task Handle(MyEvent message, IMessageHandlerContext context)
                {
                    return Task.FromResult(0);
                }

                public Task Handle(MyOtherEvent message, IMessageHandlerContext context)
                {
                    return Task.FromResult(0);
                }
            }
        }

        public class MyEvent : IEvent {}
        public class MyOtherEvent : IEvent {}
    }

    static class MD5HashBuilder
    {
        public static string Build(string input)
        {
            //use MD5 hash to get a 16-byte hash of the string
            using (var provider = new MD5CryptoServiceProvider())
            {
                var inputBytes = Encoding.Default.GetBytes(input);
                var hashBytes = provider.ComputeHash(inputBytes);

                return new Guid(hashBytes).ToString();
            }
        }
    }
}