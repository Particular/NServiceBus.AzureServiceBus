namespace NServiceBus.AcceptanceTests.Migration
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_migrating : NServiceBusAcceptanceTest
    {
        const int numberOfMessagesToSend = 5;

        [Test]
        public async Task Should_only_deliver_response_to_one_of_the_instances()
        {
            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>()
                .WithEndpoint<SubscriberUsingEndpointOrientedTopology>()
                .Done(c => c.EndpointsStarted)
                .Run();

            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>(b => 
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingEndpointOrientedTopologyWithMigration>()
                .Done(c => c.EndpointsStarted && c.EventReceived)
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
        
        public class SubscriberUsingEndpointOrientedTopologyWithMigration : EndpointConfigurationBuilder
        {
            public SubscriberUsingEndpointOrientedTopologyWithMigration()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    
                    var topology = c.ConfigureAzureServiceBus().UseMigrationTopology();
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
    
    
    public class When_migrating2 : NServiceBusAcceptanceTest
    {
        const int numberOfMessagesToSend = 5;

        [Test]
        public async Task Should_only_deliver_response_to_one_of_the_instances()
        {
            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>()
                .WithEndpoint<SubscriberUsingEndpointOrientedTopology>()
                .Done(c => c.EndpointsStarted)
                .Run();
            
            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingEndpointOrientedTopology>(b => 
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingEndpointOrientedTopologyWithMigration>(b => 
                    b.When(session => session.Publish(new MyOtherEvent())))
                .Done(c => c.EndpointsStarted && c.EventReceived && c.OtherEventReceived)
                .Run();

            await Scenario.Define<Context>()
                .WithEndpoint<PublisherUsingEndpointOrientedTopologyWithMigration>(b => 
                    b.When(session => session.Publish(new MyEvent())))
                .WithEndpoint<SubscriberUsingEndpointOrientedTopologyWithMigration>(b => 
                    b.When(session => session.Publish(new MyOtherEvent())))
                .Done(c => c.EndpointsStarted && c.EventReceived && c.OtherEventReceived)
                .Run();
        }

        public class PublisherUsingEndpointOrientedTopology : EndpointConfigurationBuilder
        {
            public PublisherUsingEndpointOrientedTopology()
            {
                EndpointSetup<DefaultPublisher>(c =>
                {
                    var topology = c.ConfigureAzureServiceBus().UseEndpointOrientedTopology();
                    topology.RegisterPublisher(typeof(MyOtherEvent), Conventions.EndpointNamingConvention(typeof(SubscriberUsingEndpointOrientedTopology)));
                });
            }
            
            class MyHandler : IHandleMessages<MyOtherEvent>
            {
                Context testContext;

                public MyHandler(Context context)
                {
                    testContext = context;
                }
                
                public Task Handle(MyOtherEvent message, IMessageHandlerContext context)
                {
                    testContext.OtherEventReceived = true;
                    return Task.CompletedTask;
                }
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
        
        public class PublisherUsingEndpointOrientedTopologyWithMigration : EndpointConfigurationBuilder
        {
            public PublisherUsingEndpointOrientedTopologyWithMigration()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    
                    var topology = c.ConfigureAzureServiceBus().UseMigrationTopology();
                    topology.RegisterPublisher(typeof(MyOtherEvent), Conventions.EndpointNamingConvention(typeof(SubscriberUsingEndpointOrientedTopology)));
                }).CustomEndpointName(Conventions.EndpointNamingConvention(typeof(PublisherUsingEndpointOrientedTopology)));
            }
            
            class MyHandler : IHandleMessages<MyOtherEvent>
            {
                Context testContext;

                public MyHandler(Context context)
                {
                    testContext = context;
                }
                
                public Task Handle(MyOtherEvent message, IMessageHandlerContext context)
                {
                    testContext.OtherEventReceived = true;
                    return Task.CompletedTask;
                }
            }
        }
        
        public class SubscriberUsingEndpointOrientedTopologyWithMigration : EndpointConfigurationBuilder
        {
            public SubscriberUsingEndpointOrientedTopologyWithMigration()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    
                    var topology = c.ConfigureAzureServiceBus().UseMigrationTopology();
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

        public class MyEvent : IEvent
        {
        }
        
        public class MyOtherEvent : IEvent
        {
        }

        public class MyResponse : IMessage
        {
            public string Client { get; set; }
        }

        public class Context : ScenarioContext
        {
            public bool EventReceived { get; set; }
            public bool OtherEventReceived { get; set; }
        }

    }
}