namespace NServiceBus.AcceptanceTests.Migration
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_migrating_single_subscriber : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_receive_at_all_stages_if_started_with_subscriber()
        {
            Requires.EndpointOrientedMigrationTopology();

            var testTimeout = TimeSpan.FromSeconds(30);

            // initial
            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingEndpointOrientedTopology>()
                .Done(c => c.EndpointsStarted && c.EventReceived)
                .Run();

            Console.WriteLine();
            Console.WriteLine("---- (1) Migrate subscriber");
            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingMigrationTopology>()
                .Done(c => c.EndpointsStarted && c.EventReceived)
                .Run(testTimeout);

            Console.WriteLine();
            Console.WriteLine("---- (2) Migrate publisher");
            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingMigrationTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingMigrationTopology>()
                .Done(c => c.EndpointsStarted && c.EventReceived)
                .Run(testTimeout);

            Console.WriteLine();
            Console.WriteLine("---- (3) Migrate subscriber to forwarding topology");
            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingMigrationTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingForwardingTopology>()
                .Done(c => c.EndpointsStarted && c.EventReceived)
                .Run(testTimeout);

            Console.WriteLine();
            Console.WriteLine("---- (4) Migrate publisher to forwarding topology");
            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingForwardingTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingForwardingTopology>()
                .Done(c => c.EndpointsStarted && c.EventReceived)
                .Run(testTimeout);
        }

        [Test]
        public async Task Should_receive_at_all_stages_if_started_with_publisher()
        {
            Requires.EndpointOrientedMigrationTopology();

            var testTimeout = TimeSpan.FromSeconds(30);

            // initial
            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingEndpointOrientedTopology>()
                .Done(c => c.EndpointsStarted && c.EventReceived)
                .Run();

            Console.WriteLine();
            Console.WriteLine("---- (1) Migrate publisher");
            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingMigrationTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingEndpointOrientedTopology>()
                .Done(c => c.EndpointsStarted && c.EventReceived)
                .Run(testTimeout);

            Console.WriteLine();
            Console.WriteLine("---- (2) Migrate subscriber");
            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingMigrationTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingMigrationTopology>()
                .Done(c => c.EndpointsStarted && c.EventReceived)
                .Run(testTimeout);

            Console.WriteLine();
            Console.WriteLine("---- (3) Migrate publisher to forwarding topology");
            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingForwardingTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingMigrationTopology>()
                .Done(c => c.EndpointsStarted && c.EventReceived)
                .Run(testTimeout);

            Console.WriteLine();
            Console.WriteLine("---- (4) Migrate subscriber to forwarding topology");
            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingForwardingTopology>(b =>
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingForwardingTopology>()
                .Done(c => c.EndpointsStarted && c.EventReceived)
                .Run(testTimeout);
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
                    testContext.EventReceived = true;
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
                    testContext.EventReceived = true;
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
                    testContext.EventReceived = true;
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

        public class Context : ScenarioContext
        {
            public bool EventReceived { get; set; }
        }

    }
}