namespace NServiceBus.AcceptanceTests.Routing
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_using_a_single_bundle : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_receive_events()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<Subscriber>()
                .WithEndpoint<Publisher>(b => b.When((session, c) => session.Publish(new MyEvent())))
                .Done(c => c.SubscriberGotTheEvent)
                .Run();

            Assert.IsTrue(context.SubscriberGotTheEvent);
        }

        public class Context : ScenarioContext
        {
            public bool SubscriberGotTheEvent { get; set; }
        }

        public class Publisher : EndpointConfigurationBuilder
        {
            public Publisher()
            {
                EndpointSetup<DefaultServer>(config =>
                {
                    var transport = config.UseTransport<AzureServiceBusTransport>();
                    var topology = transport.UseForwardingTopology();
#pragma warning disable 618
                    topology.NumberOfEntitiesInBundle(1);
#pragma warning restore 618
                });
            }
        }

        public class Subscriber : EndpointConfigurationBuilder
        {
            public Subscriber()
            {
                EndpointSetup<DefaultServer>(config =>
                {
                    var transport = config.UseTransport<AzureServiceBusTransport>();
                    var topology = transport.UseForwardingTopology();
#pragma warning disable 618
                    topology.NumberOfEntitiesInBundle(1);
#pragma warning restore 618
                    transport.Routing().RouteToEndpoint(typeof(MyEvent), typeof(Publisher));
                });
            }

            public class MyEventHandler : IHandleMessages<MyEvent>
            {
                public Context TestContext { get; set; }

                public Task Handle(MyEvent message, IMessageHandlerContext context)
                {
                    TestContext.SubscriberGotTheEvent = true;
                    return Task.FromResult(0);
                }
            }
        }

        public class MyEvent : IEvent
        {
        }
    }
}