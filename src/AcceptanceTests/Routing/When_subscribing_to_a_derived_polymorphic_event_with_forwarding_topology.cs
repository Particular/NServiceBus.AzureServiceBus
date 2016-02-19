namespace NServiceBus.AcceptanceTests.Routing
{
    using System.Threading.Tasks;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Features;
    using NServiceBus.Settings;
    using NUnit.Framework;

    public class When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_handle_each_event_once_only()
        {
            var context = await Scenario.Define<Context>()
                    .WithEndpoint<Publisher>(b => b.When(bus => bus.Publish<DerivedEvent>()))
                    .WithEndpoint<Subscriber>(b => b.When(async session =>
                    {
                        await session.Subscribe<BaseEvent>();
                        await session.Subscribe<DerivedEvent>();
                    }))
                    .Run();

            if (!context.IsForwardingTopology)
            {
                Assert.Inconclusive($"The test is designed for {typeof(ForwardingTopology).Name} only.");
            }

            Assert.That(context.SubscriberGotTheBaseEvent, Is.EqualTo(1), $"Should only receive BaseEvent once, but it was {context.SubscriberGotTheBaseEvent}");
            Assert.That(context.SubscriberGotTheDerivedEvent, Is.EqualTo(1), $"Should only receive DerivedEvent once, but it was {context.SubscriberGotTheDerivedEvent}");
        }

        public class Context : ScenarioContext
        {
            public int SubscriberGotTheDerivedEvent { get; set; }

            public int SubscriberGotTheBaseEvent { get; set; }
            public bool IsForwardingTopology { get; set; }
        }

        public class Publisher : EndpointConfigurationBuilder
        {
            public Context Context { get; set; }

            public Publisher()
            {
                EndpointSetup<DefaultPublisher>();
            }

            class DetermineWhatTopologyIsUsed : IWantToRunWhenBusStartsAndStops
            {
                public Context Context { get; set; }

                public ReadOnlySettings Settings { get; set; }

                public Task Start(IBusSession session)
                {
                    Context.IsForwardingTopology = Settings.Get<ITopology>() is ForwardingTopology;
                    return Task.FromResult(0);
                }

                public Task Stop(IBusSession session)
                {
                    return Task.FromResult(0);
                }
            }
        }

        public class Subscriber : EndpointConfigurationBuilder
        {
            public Subscriber()
            {
                EndpointSetup<DefaultServer>(busConfiguration => busConfiguration.DisableFeature<AutoSubscribe>())
                    .AddMapping<BaseEvent>(typeof(Publisher))
                    .AddMapping<DerivedEvent>(typeof(Publisher));
            }

            public class MyEventHandler : IHandleMessages<DerivedEvent>, IHandleMessages<BaseEvent>
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
            }
        }

        public interface BaseEvent : IEvent
        {
        }

        public interface DerivedEvent : BaseEvent
        {
        }
    }
}