namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transports;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_routing_outgoingmessages_to_endpoints
    {
        [Test]
        public async Task Can_route_an_outgoing_message()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator();
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new ClientEntityLifeCycleManager(messageSenderCreator, settings);

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = namespaceManagerLifeCycleManager.Get(AzureServiceBusConnectionString.Value);
            await creator.CreateAsync("myqueue", namespaceManager);

            //// perform the test

            var router = new DefaultOutgoingMessageRouter(
                new FakeAddressingStrategy(), //TODO: Is this the same as IProvideDynamicRouting?
                new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings), // this feels odd that brokeredmessage is a concern at this level, should be implementation detail
                clientLifecycleManager, settings
                );

            var bytes = Encoding.UTF8.GetBytes("Whatever");
            var outgoingMessage = new OutgoingMessage("SomeId", new Dictionary<string, string>(), bytes);
            var dispatchOptions = new DispatchOptions(new DirectToTargetDestination("MyQueue"), null, new List<DeliveryConstraint>());

            
            await router.RouteAsync(outgoingMessage, dispatchOptions);

            //validate
            var queue = await namespaceManager.GetQueueAsync("myqueue");
            Assert.IsTrue(queue.MessageCount > 0);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }

        [Test]
        public async Task Can_route_an_outgoing_batch_of_message()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator();
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new ClientEntityLifeCycleManager(messageSenderCreator, settings);

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = namespaceManagerLifeCycleManager.Get(AzureServiceBusConnectionString.Value);
            await creator.CreateAsync("myqueue", namespaceManager);

            //// perform the test

            var router = new DefaultOutgoingMessageRouter(
                new FakeAddressingStrategy(), //TODO: Is this the same as IProvideDynamicRouting?
                new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings), // this feels odd that brokeredmessage is a concern at this level, should be implementation detail
                clientLifecycleManager, settings
                );

            var bytes = Encoding.UTF8.GetBytes("Whatever");
            var outgoingMessage1 = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);
            var outgoingMessage2 = new OutgoingMessage("Id-2", new Dictionary<string, string>(), bytes);
            var dispatchOptions = new DispatchOptions(new DirectToTargetDestination("MyQueue"), null, Enumerable.Empty<DeliveryConstraint>());
            
            await router.RouteBatchAsync(new [] {outgoingMessage1, outgoingMessage2}, dispatchOptions);

            //validate
            var queue = await namespaceManager.GetQueueAsync("myqueue");
            Assert.IsTrue(queue.MessageCount == 2);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }

        public class FakeAddressingStrategy : IAddressingStrategy
        {
            public EntityInfo[] GetEntitiesForPublishing(Type eventType)
            {
                throw new NotImplementedException();
            }

            public EntityInfo[] GetEntitiesForSending(string destination)
            {
                return new[]
                {
                    new EntityInfo
                    {
                        Path = destination,
                        Namespace = new NamespaceInfo(AzureServiceBusConnectionString.Value, NamespaceMode.Active)
                    }
                };
            }
        }
    }
}