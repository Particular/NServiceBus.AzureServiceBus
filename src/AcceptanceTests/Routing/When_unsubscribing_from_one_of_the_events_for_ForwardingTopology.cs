namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using Features;
    using Microsoft.ServiceBus;
    using NUnit.Framework;
    using Settings;
    using Transport.AzureServiceBus;
    using AzureServiceBus;
    using NServiceBus.AcceptanceTests;
    using AcceptanceTesting.Customization;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;

    public class When_unsubscribing_from_one_of_the_events_for_ForwardingTopology : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_remove_subscription_rule_from_topology()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<Endpoint>(b => b.When((session, c) => session.Unsubscribe<MyEvent>()))
                .Done(c => c.EndpointsStarted)
                .Run();

            var connectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            var rawEndpointName = Conventions.EndpointNamingConvention(typeof(Endpoint));
            var endpointName = MD5HashBuilder.Build(rawEndpointName);
            var sanitizedEventFullName = typeof(MyOtherEvent).FullName.Replace("+", string.Empty);
            var otherRuleName = MD5HashBuilder.Build(sanitizedEventFullName);

            if (context.IsForwardingTopology)
            {
                var isSubscriptionFound = await namespaceManager.SubscriptionExistsAsync("bundle-1", endpointName);
                Assert.IsTrue(isSubscriptionFound, "Subscription under 'bundle-1' should have been found, but it wasn't.");

                isSubscriptionFound = await namespaceManager.SubscriptionExistsAsync("bundle-2", endpointName);
                Assert.IsTrue(isSubscriptionFound, "Subscription under 'bundle-2' should have been found, but it wasn't.");

                var rules = await namespaceManager.GetRulesAsync("bundle-1", endpointName);
                CollectionAssert.AreEquivalent(new[] { otherRuleName }, rules.Select(r => r.Name));

                rules = await namespaceManager.GetRulesAsync("bundle-2", endpointName);
                CollectionAssert.AreEquivalent(new [] { otherRuleName }, rules.Select(r => r.Name));
            }
            else
            {
                Assert.Ignore("Not applicable to EndpointOrientedTopology");
            }
        }

        public class Context : ScenarioContext
        {
            public bool IsForwardingTopology { get; set; }
        }

        public class Endpoint : EndpointConfigurationBuilder
        {
            public Endpoint()
            {
                EndpointSetup<DefaultServer>(endpointConfiguration =>
                {
                    endpointConfiguration.EnableFeature<DetermineWhatTopologyIsUsed>();
                    endpointConfiguration.ConfigureTransport().Routing().RouteToEndpoint(typeof(MyEvent), typeof(Endpoint));
                    endpointConfiguration.ConfigureTransport().Routing().RouteToEndpoint(typeof(MyOtherEvent), typeof(Endpoint));
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

            class DetermineWhatTopologyIsUsed : Feature
            {
                protected override void Setup(FeatureConfigurationContext context)
                {
                    context.RegisterStartupTask(builder => new TaskToDetermineCurrentTopology(builder.Build<Context>(), builder.Build<ReadOnlySettings>()));
                }
            }

            class TaskToDetermineCurrentTopology : FeatureStartupTask
            {
                Context context;
                ReadOnlySettings settings;

                public TaskToDetermineCurrentTopology(Context context, ReadOnlySettings settings)
                {
                    this.context = context;
                    this.settings = settings;
                }

                protected override Task OnStart(IMessageSession session)
                {
#pragma warning disable 618
                    context.IsForwardingTopology = settings.Get<ITopology>() is ForwardingTopology;
#pragma warning restore 618
                    return TaskEx.Completed;
                }

                protected override Task OnStop(IMessageSession session)
                {
                    return TaskEx.Completed;
                }
            }
        }

        public class MyEvent : IEvent
        {
        }
        public class MyOtherEvent : IEvent
        {
        }
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