namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using Features;
    using NUnit.Framework;

    public class When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_handle_each_event_once_only()
        {
            Requires.ForwardingToplogy();

            var context = await Scenario.Define<Context>()
                // Publish event only when it was signaled that events went out
                .WithEndpoint<Publisher>(b => b.When(c => c.ReceiverSubscribedToEvents, async bus =>
                {
                    await bus.Publish<DerivedEvent>();

                    // Delay signal command to stop by 2 seconds to ensure there was enough time for duplicate events
                    var sendOptions = new SendOptions();
                    sendOptions.DelayDeliveryWith(TimeSpan.FromSeconds(2));
                    await bus.Send(new EventWasRaisedSoStopProcessing(), sendOptions);
                }))
                .WithEndpoint<Subscriber>(b => b.When(async (session, c) =>
                {
                    await session.Subscribe<BaseEvent>();
                    await session.Subscribe<DerivedEvent>();

                    c.ReceiverSubscribedToEvents = true;
                }))
                .Done(ctx => ctx.StopCommandWasReceived)
                .Run();

            Assert.That(context.SubscriberGotTheBaseEvent, Is.EqualTo(1), $"Should only receive BaseEvent once, but it was {context.SubscriberGotTheBaseEvent}");
            Assert.That(context.SubscriberGotTheDerivedEvent, Is.EqualTo(1), $"Should only receive DerivedEvent once, but it was {context.SubscriberGotTheDerivedEvent}");
        }

        public class Context : ScenarioContext
        {
            public int SubscriberGotTheDerivedEvent { get; set; }
            public int SubscriberGotTheBaseEvent { get; set; }
            public bool ReceiverSubscribedToEvents { get; set; }
            public bool StopCommandWasReceived { get; set; }
        }

        public class Publisher : EndpointConfigurationBuilder
        {
            public Context Context { get; set; }

            public Publisher()
            {
                EndpointSetup<DefaultPublisher>(c =>
                {
                    c.ConfigureTransport().Routing().RouteToEndpoint(typeof(EventWasRaisedSoStopProcessing), typeof(Subscriber));
                });
            }
        }


        public class Subscriber : EndpointConfigurationBuilder
        {
            public Subscriber()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    c.DisableFeature<AutoSubscribe>();
                    // Limit message processing to a single thread to ensure that if duplicate events are sent, they are getting
                    // to the subscriber before stop command
                    c.LimitMessageProcessingConcurrencyTo(1);
                });
            }

            public class MyEventHandler : IHandleMessages<DerivedEvent>, IHandleMessages<BaseEvent>, IHandleMessages<EventWasRaisedSoStopProcessing>
            {
                public Context Context { get; set; }

                public Task Handle(DerivedEvent message, IMessageHandlerContext context)
                {
                    Context.SubscriberGotTheDerivedEvent++;
                    return Task.FromResult(0);
                }

                public Task Handle(BaseEvent message, IMessageHandlerContext context)
                {
                    Context.SubscriberGotTheBaseEvent++;
                    return Task.FromResult(0);
                }

                public Task Handle(EventWasRaisedSoStopProcessing message, IMessageHandlerContext context)
                {
                    Context.StopCommandWasReceived = true;
                    return Task.FromResult(0);
                }
            }
        }

        public interface BaseEvent : IEvent
        {
        }

        public interface DerivedEvent : BaseEvent
        {
        }

        public class EventWasRaisedSoStopProcessing : ICommand
        {

        }
    }
}