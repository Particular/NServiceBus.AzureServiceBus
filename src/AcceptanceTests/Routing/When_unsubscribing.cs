namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using Features;
    using Microsoft.ServiceBus;
    using NUnit.Framework;
    using Settings;
    using Transport.AzureServiceBus;
    using AzureServiceBus;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;

    public class When_unsubscribing : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_remove_subscription_from_topology()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<Endpoint>(b => b.When((session, c) => session.Unsubscribe<MyEvent>()))
                .Done(c => c.EndpointsStarted)
                .Run();

            var connectionString = Environment.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!context.IsForwardingTopology)
            {
                var endpointName = ConfigureEndpointAzureServiceBusTransport.NameForEndpoint<Endpoint>();
                var isSubscriptionFound = await namespaceManager.SubscriptionExistsAsync(endpointName + ".events", $"{endpointName}.{nameof(MyEvent)}");
                Assert.IsFalse(isSubscriptionFound, "Subscription should have been deleted, but it wasn't.");
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
                EndpointSetup<DefaultServer>()
                    .AddMapping<MyEvent>(typeof(Endpoint));
            }

            public class Handler : IHandleMessages<MyEvent>
            {
                public Task Handle(MyEvent message, IMessageHandlerContext context)
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
    }
}