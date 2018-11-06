namespace NServiceBus.AcceptanceTests.Migration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using AzureServiceBus.AcceptanceTests.Infrastructure;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_migrating_single_subscriber_with_msgs_in_subscription : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_receive_at_all_stages_if_started_with_subscriber()
        {
            // initial
            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingEndpointOrientedTopology>()
                .Done(c => c.EndpointsStarted && c.EventsReceived == 1)
                .Run();

            var overrideTestRunId = Guid.NewGuid();

            await Scenario.Define<Context>(c => c.OverrideTestRunId = overrideTestRunId)
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>(
                    b => b.When(session => session.Publish(new MyEvent())))
                .Done(c => c.EndpointsStarted)
                .Run();

            // subscriber migrated
            await Scenario.Define<Context>(c => c.OverrideTestRunId = overrideTestRunId)
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingMigrationTopology>()
                .Done(c => c.EndpointsStarted && c.EventsReceived == 2)
                .Run();

            await Scenario.Define<Context>(c => c.OverrideTestRunId = overrideTestRunId)
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>(
                    b => b.When(session => session.Publish(new MyEvent())))
                .Done(c => c.EndpointsStarted)
                .Run();

            // publisher migrated
            await Scenario.Define<Context>(c => c.OverrideTestRunId = overrideTestRunId)
                .WithEndpoint<PublisherUsingMigrationTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingMigrationTopology>()
                .Done(c => c.EndpointsStarted && c.EventsReceived == 2)
                .Run();

            await Scenario.Define<Context>(c => c.OverrideTestRunId = overrideTestRunId)
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>(
                    b => b.When(session => session.Publish(new MyEvent())))
                .Done(c => c.EndpointsStarted)
                .Run();

            // subscriber on forwarding
            await Scenario.Define<Context>(c => c.OverrideTestRunId = overrideTestRunId)
                .WithEndpoint<PublisherUsingMigrationTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingForwardingTopology>()
                .Done(c => c.EndpointsStarted && c.EventsReceived == 2)
                .Run();

            await Scenario.Define<Context>(c => c.OverrideTestRunId = overrideTestRunId)
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>(
                    b => b.When(session => session.Publish(new MyEvent())))
                .Done(c => c.EndpointsStarted)
                .Run();

            // publisher on forwarding
            await Scenario.Define<Context>(c => c.OverrideTestRunId = overrideTestRunId)
                .WithEndpoint<PublisherUsingForwardingTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingForwardingTopology>()
                .Done(c => c.EndpointsStarted && c.EventsReceived == 2)
                .Run();
        }

        [Test]
        public async Task Should_receive_at_all_stages_if_started_with_publisher()
        {
            // initial
            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingEndpointOrientedTopology>()
                .Done(c => c.EndpointsStarted && c.EventsReceived == 1)
                .Run();

            var overrideTestRunId = Guid.NewGuid();

            await Scenario.Define<Context>(c => c.OverrideTestRunId = overrideTestRunId)
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>(
                    b => b.When(session => session.Publish(new MyEvent())))
                .Done(c => c.EndpointsStarted)
                .Run();

            // publisher migrated
            await Scenario.Define<Context>(c => c.OverrideTestRunId = overrideTestRunId)
                .WithEndpoint<PublisherUsingMigrationTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingEndpointOrientedTopology>()
                .Done(c => c.EndpointsStarted && c.EventsReceived == 2)
                .Run();

            await Scenario.Define<Context>(c => c.OverrideTestRunId = overrideTestRunId)
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>(
                    b => b.When(session => session.Publish(new MyEvent())))
                .Done(c => c.EndpointsStarted)
                .Run();

            // subscriber migrated
            await Scenario.Define<Context>(c => c.OverrideTestRunId = overrideTestRunId)
                .WithEndpoint<PublisherUsingMigrationTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingMigrationTopology>()
                .Done(c => c.EndpointsStarted && c.EventsReceived == 2)
                .Run();

            await Scenario.Define<Context>(c => c.OverrideTestRunId = overrideTestRunId)
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>(
                    b => b.When(session => session.Publish(new MyEvent())))
                .Done(c => c.EndpointsStarted)
                .Run();

            // publisher on forwarding
            await Scenario.Define<Context>(c => c.OverrideTestRunId = overrideTestRunId)
                .WithEndpoint<PublisherUsingForwardingTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingMigrationTopology>()
                .Done(c => c.EndpointsStarted && c.EventsReceived == 2)
                .Run();

            await Scenario.Define<Context>(c => c.OverrideTestRunId = overrideTestRunId)
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>(
                    b => b.When(session => session.Publish(new MyEvent())))
                .Done(c => c.EndpointsStarted)
                .Run();

            // subscriber on forwarding
            await Scenario.Define<Context>(c => c.OverrideTestRunId = overrideTestRunId)
                .WithEndpoint<PublisherUsingForwardingTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingForwardingTopology>()
                .Done(c => c.EndpointsStarted && c.EventsReceived == 2)
                .Run();
        }

        public class PublisherUsingEndpointOrientedTopology : EndpointConfigurationBuilder
        {
            public PublisherUsingEndpointOrientedTopology()
            {
                EndpointSetup<DefaultPublisher>(c =>
                    c.ConfigureAzureServiceBus().UseEndpointOrientedTopology());
            }
        }

        public class PublisherUsingMigrationTopology : EndpointConfigurationBuilder
        {
            public PublisherUsingMigrationTopology()
            {
                EndpointSetup<DefaultPublisher>(c =>
                        c.ConfigureAzureServiceBus().UseEndpointOrientedTopology().EnableMigrationToForwardingTopology())
                    .CustomEndpointName(Conventions.EndpointNamingConvention(typeof(PublisherUsingEndpointOrientedTopology)));
            }
        }

        public class PublisherUsingForwardingTopology : EndpointConfigurationBuilder
        {
            public PublisherUsingForwardingTopology()
            {
                EndpointSetup<DefaultPublisher>(c =>
                        c.ConfigureAzureServiceBus().UseForwardingTopology())
                    .CustomEndpointName(Conventions.EndpointNamingConvention(typeof(PublisherUsingEndpointOrientedTopology)));
            }
        }

        public class SubscriberUsingEndpointOrientedTopology : EndpointConfigurationBuilder
        {
            public SubscriberUsingEndpointOrientedTopology()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    var topology = c.ConfigureAzureServiceBus().UseEndpointOrientedTopology();
                    topology.RegisterPublisher(typeof(MyEvent), Conventions.EndpointNamingConvention(typeof(PublisherUsingEndpointOrientedTopology)));
                });
            }

            class MyHandler : IHandleMessages<MyEvent>
            {
                Context testContext;

                public MyHandler(Context context)
                {
                    testContext = context;
                }

                public Task Handle(MyEvent message, IMessageHandlerContext context)
                {
                    testContext.ReceivedEvent();
                    return Task.CompletedTask;
                }
            }
        }

        public class SubscriberUsingMigrationTopology : EndpointConfigurationBuilder
        {
            public SubscriberUsingMigrationTopology()
            {
                EndpointSetup<DefaultServer>(c =>
                {

                    var topology = c.ConfigureAzureServiceBus().UseEndpointOrientedTopology().EnableMigrationToForwardingTopology();
                    topology.RegisterPublisher(typeof(MyEvent), Conventions.EndpointNamingConvention(typeof(PublisherUsingEndpointOrientedTopology)));
                }).CustomEndpointName(Conventions.EndpointNamingConvention(typeof(SubscriberUsingEndpointOrientedTopology)));
            }

            class MyHandler : IHandleMessages<MyEvent>
            {
                Context testContext;

                public MyHandler(Context context)
                {
                    testContext = context;
                }

                public Task Handle(MyEvent message, IMessageHandlerContext context)
                {
                    testContext.ReceivedEvent();
                    return Task.CompletedTask;
                }
            }
        }

        public class SubscriberUsingForwardingTopology : EndpointConfigurationBuilder
        {
            public SubscriberUsingForwardingTopology()
            {
                EndpointSetup<DefaultServer>(c =>
                {

                    c.ConfigureAzureServiceBus().UseForwardingTopology();
                }).CustomEndpointName(Conventions.EndpointNamingConvention(typeof(SubscriberUsingEndpointOrientedTopology)));
            }

            class MyHandler : IHandleMessages<MyEvent>
            {
                Context testContext;

                public MyHandler(Context context)
                {
                    testContext = context;
                }

                public Task Handle(MyEvent message, IMessageHandlerContext context)
                {
                    testContext.ReceivedEvent();
                    return Task.CompletedTask;
                }
            }
        }

        public class MyEvent : IEvent
        {
        }

        public class MyResponse : IMessage
        {
            public string Client { get; set; }
        }

        public class Context : ScenarioContext, IProvideTestRunId
        {
            long eventsReceived;
            public long EventsReceived => Interlocked.Read(ref eventsReceived);

            public Guid? OverrideTestRunId { get; set; }

            public void ReceivedEvent()
            {
                Interlocked.Increment(ref eventsReceived);
            }
        }

    }
}