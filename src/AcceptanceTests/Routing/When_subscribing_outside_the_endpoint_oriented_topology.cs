namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Support;
    using AzureServiceBus;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using Features;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;
    using Settings;
    using NUnit.Framework;

    public class When_subscribing_outside_the_endpoint_oriented_topology : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_receive_event()
        {
            var runSettings = new RunSettings();
            runSettings.TestExecutionTimeout = TimeSpan.FromMinutes(1);

            var config = new AzureServiceBusTransportConfigContext();
            config.Callback = (endpointName, extensions) =>
            {
                var publisherConnectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");
                var subscriberConnectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBus.ConnectionString.Fallback");

                if (endpointName == "SubscribingOutsideTheEndpointOrientedTopology.Subscriber")
                {

                    extensions.NamespacePartitioning().AddNamespace("subscriberNamespace", subscriberConnectionString);
                    extensions.NamespaceRouting()
                                    .AddNamespace("publisherNamespace", publisherConnectionString)
                                    .RegisteredEndpoints.Add(AcceptanceTesting.Customization.Conventions.EndpointNamingConvention(typeof(Publisher)));
                }
                else
                {
                    extensions.NamespacePartitioning().AddNamespace("publisherNamespace", publisherConnectionString);
                }

                var topology = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBusTransport.Topology");
                if (topology == "ForwardingTopology")
                {
                    extensions.UseForwardingTopology();
                }
                else
                {
                    if (endpointName == "SubscribingOutsideTheEndpointOrientedTopology.Subscriber")
                    {
                        extensions.UseEndpointOrientedTopology()
                            .RegisterPublisher(typeof(MyEvent), AcceptanceTesting.Customization.Conventions.EndpointNamingConvention(typeof(Publisher)));
                    }
                    else
                    {
                        extensions.UseEndpointOrientedTopology();
                    }
                }
            };

            runSettings.Set("AzureServiceBus.AcceptanceTests.TransportConfigContext", config);

            var context = await Scenario.Define<Context>()
                // Publish event only when it was signaled that events went out
                .WithEndpoint<Publisher>(b => b.When(c => c.ReceiverSubscribedToEvents, async bus =>
                {
                    await bus.Publish<MyEvent>();
                }))
                .WithEndpoint<Subscriber>(b => b.When(async (session, c) =>
                {
                    await session.Subscribe<MyEvent>();

                    c.ReceiverSubscribedToEvents = true;
                }))
                .Done(ctx => ctx.SubscriberGotTheEvent || !ctx.IsEndpointOrientedTopology)
                .Run(runSettings);

            if (!context.IsEndpointOrientedTopology)
            {
                Assert.Inconclusive("The test is designed for EndpointOrientedTopology only.");
            }

            Assert.That(context.SubscriberGotTheEvent, Is.True, "Should receive the event");
         }

        public class Context : ScenarioContext
        {
            public bool SubscriberGotTheEvent { get; set; }
            public bool IsEndpointOrientedTopology { get; set; }
            public bool ReceiverSubscribedToEvents { get; set; }
        }

        public class Publisher : EndpointConfigurationBuilder
        {
            public Context Context { get; set; }

            public Publisher()
            {
                EndpointSetup<DefaultPublisher>(endpointConfiguration =>
                {
                    endpointConfiguration.EnableFeature<DetermineWhatTopologyIsUsed>();
                });
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
                    context.IsEndpointOrientedTopology = settings.Get<string>("AzureServiceBus.AcceptanceTests.UsedTopology") == "EndpointOrientedTopology";
                    return TaskEx.Completed;
                }

                protected override Task OnStop(IMessageSession session)
                {
                    return TaskEx.Completed;
                }
            }
        }


        public class Subscriber : EndpointConfigurationBuilder
        {
            public Subscriber()
            {
                EndpointSetup<DefaultServer>(endpointConfiguration =>
                {
                    endpointConfiguration.DisableFeature<AutoSubscribe>();
                });
            }

            public class MyEventHandler : IHandleMessages<MyEvent>
            {
                public Context Context { get; set; }

                public Task Handle(MyEvent message, IMessageHandlerContext context)
                {
                    Context.SubscriberGotTheEvent =  true;
                    return Task.FromResult(0);
                }
            }
        }

        public interface MyEvent : IEvent
        {
        }
    }
}