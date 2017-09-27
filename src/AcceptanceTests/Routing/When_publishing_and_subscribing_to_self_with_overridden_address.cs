namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_publishing_and_subscribing_to_self_with_overridden_address : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_be_delivered_to_self()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<PublisherAndSubscriber>(builder => 
                    builder.When(session => session.Publish(new MyEvent()))) 
                .Done(c => c.GotTheEvent)
                .Run();

            Assert.True(context.GotTheEvent);
        }

        public class Context : ScenarioContext
        {
            public bool GotTheEvent { get; set; }
            public bool IsSubscribed { get; set; }
        }

        public class PublisherAndSubscriber : EndpointConfigurationBuilder
        {
            public PublisherAndSubscriber()
            {
                EndpointSetup<DefaultPublisher>(builder =>
                {
                    builder.OverrideLocalAddress("myinputqueue");                    
                });
            }

            public class MyEventHandler : IHandleMessages<MyEvent>
            {
                public Context Context { get; set; }

                public Task Handle(MyEvent messageThatIsEnlisted, IMessageHandlerContext context)
                {
                    Context.GotTheEvent = true;
                    return Task.FromResult(0);
                }
            }
        }

        public class MyEvent : IEvent
        {
        }
    }
}