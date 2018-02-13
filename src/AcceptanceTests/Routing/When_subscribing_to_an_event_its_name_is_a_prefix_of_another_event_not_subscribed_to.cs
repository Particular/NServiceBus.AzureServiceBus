namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_subscribing_to_an_event_its_name_is_a_prefix_of_another_event_not_subscribed_to : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_not_subscribe_to_another_event()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<Subscriber>()
                .WithEndpoint<ErrorSpy>()
                .WithEndpoint<Publisher>(b => b.When(async bus =>
                {
                    await bus.Publish<EventSubscriberNotInterestedIn>();
                }))
                .Done(ctx => ctx.ReceivedIncorrectEvent)
                .Run();

            Assert.That(context.ReceivedIncorrectEvent, Is.False, $"Subscriber should never receive {nameof(EventSubscriberNotInterestedIn)} event, but it did.");
        }

        public class Context : ScenarioContext
        {
            public bool ReceivedIncorrectEvent { get; set; }
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
                EndpointSetup<DefaultServer>(c => c.SendFailedMessagesTo("error"), 
                    publisherMetadata =>
                    {
                        publisherMetadata.RegisterPublisherFor<Event>(typeof(Publisher));
                        publisherMetadata.RegisterPublisherFor<EventSubscriberNotInterestedIn>(typeof(Publisher));
                    });
            }

            public class MyEventHandler : IHandleMessages<Event>
            {
                public Context Context { get; set; }

                public Task Handle(Event message, IMessageHandlerContext context)
                {
                    return Task.FromResult(0);
                }
            }
        }

        class ErrorSpy : EndpointConfigurationBuilder
        {
            public ErrorSpy()
            {
                EndpointSetup<DefaultServer>().CustomEndpointName("error");
            }

            class Handler : IHandleMessages<EventSubscriberNotInterestedIn>
            {
                public Context TestContext { get; set; }

                public Task Handle(EventSubscriberNotInterestedIn message, IMessageHandlerContext context)
                {
                    TestContext.ReceivedIncorrectEvent = true;
                    return Task.FromResult(0);
                }
            }
        }


        public interface Event : IEvent
        {
        }

        public interface EventSubscriberNotInterestedIn : IEvent
        {
        }
    }
}