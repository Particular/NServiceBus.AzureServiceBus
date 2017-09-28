namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using Conventions = AcceptanceTesting.Customization.Conventions;
    using Features;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;
    using NUnit.Framework;

    public class When_subscribing_outside_the_endpoint_oriented_topology : NServiceBusAcceptanceTest
    {
        static string publisherConnectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");
        static string subscriberConnectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBus.ConnectionString.Fallback");

        [Test]
        public async Task Should_receive_event()
        {
            TestRequires.EndpointOrientedToplogy();

            var context = await Scenario.Define<Context>()
                // Publish event only when it was signaled that events went out
                .WithEndpoint<Publisher>(b =>
                {
                    b.CustomConfig(c =>
                    {
                        var transport = c.ConfigureAzureServiceBus();
                        transport.NamespacePartitioning().AddNamespace("publisherNamespace", publisherConnectionString);
                        transport.UseEndpointOrientedTopology();
                    })
                    .When(c => c.ReceiverSubscribedToEvents, async messageSession =>
                    {
                        await messageSession.Publish<MyEvent>();
                    });
                })
                .WithEndpoint<Subscriber>(b =>
                {
                    b.CustomConfig(c =>
                    {
                        var transport = c.ConfigureAzureServiceBus();
                        transport.NamespacePartitioning().AddNamespace("subscriberNamespace", subscriberConnectionString);
                        var namespaceInfo = transport.NamespaceRouting().AddNamespace("publisherNamespace", publisherConnectionString);
                        namespaceInfo.RegisteredEndpoints.Add(Conventions.EndpointNamingConvention(typeof(Publisher)));
                        transport.UseEndpointOrientedTopology()
                            .RegisterPublisher(typeof(MyEvent), Conventions.EndpointNamingConvention(typeof(Publisher)));
                    })
                    .When(async (session, c) =>
                    {
                        await session.Subscribe<MyEvent>();

                        c.ReceiverSubscribedToEvents = true;
                    });
                })
                .Done(ctx => ctx.SubscriberGotTheEvent)
                .Run();

            Assert.That(context.SubscriberGotTheEvent, Is.True, "Should receive the event");
         }

        public class Context : ScenarioContext
        {
            public bool SubscriberGotTheEvent { get; set; }
            public bool ReceiverSubscribedToEvents { get; set; }
        }

        public class Publisher : EndpointConfigurationBuilder
        {
            public Context Context { get; set; }

            public Publisher()
            {
                EndpointSetup<DefaultPublisher>();
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