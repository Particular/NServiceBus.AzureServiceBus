namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transports;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_dispatching_messages
    {

        [Test]
        public async Task Should_dispatch_multiple_batches()
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
            var router = new DefaultOutgoingMessageRouter(new FakeAddressingStrategy(), new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings), clientLifecycleManager, settings);

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = namespaceManagerLifeCycleManager.Get(AzureServiceBusConnectionString.Value);
            await creator.CreateAsync("myqueue", namespaceManager);
            await creator.CreateAsync("myqueue2", namespaceManager);

            var bytes = Encoding.UTF8.GetBytes("Whatever");
            var outgoingMessage1 = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);
            var outgoingMessage2 = new OutgoingMessage("Id-2", new Dictionary<string, string>(), bytes);
            var outgoingMessage3 = new OutgoingMessage("Id-3", new Dictionary<string, string>(), bytes);
            var outgoingMessage4 = new OutgoingMessage("Id-4", new Dictionary<string, string>(), bytes);
            var dispatchOptions1 = new DispatchOptions(new DirectToTargetDestination("MyQueue"), null, Enumerable.Empty<DeliveryConstraint>());
            var dispatchOptions2 = new DispatchOptions(new DirectToTargetDestination("MyQueue2"), null, Enumerable.Empty<DeliveryConstraint>());

            //// perform the test
            var dispatcher = new Dispatcher(router);
            await dispatcher.Dispatch(new []
            {
                new TransportOperation(outgoingMessage1, dispatchOptions1), new TransportOperation(outgoingMessage2, dispatchOptions1), //batch #1
                new TransportOperation(outgoingMessage3, dispatchOptions2), new TransportOperation(outgoingMessage4, dispatchOptions2), //batch #2
            });

            //validate
            var queue = await namespaceManager.GetQueueAsync("myqueue");
            Assert.IsTrue(queue.MessageCount == 2);
            queue = await namespaceManager.GetQueueAsync("myqueue2");
            Assert.IsTrue(queue.MessageCount == 2);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
            await namespaceManager.DeleteQueueAsync("myqueue2");
        }

        [Test]
        public async Task Should_throw_if_at_least_one_batch_fails()
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
            var router = new DefaultOutgoingMessageRouter(new FakeAddressingStrategy(), new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings), clientLifecycleManager, settings);

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = namespaceManagerLifeCycleManager.Get(AzureServiceBusConnectionString.Value);
            await creator.CreateAsync("myqueue", namespaceManager);

            var bytes = Encoding.UTF8.GetBytes("Whatever");
            var outgoingMessage1 = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);
            var outgoingMessage2 = new OutgoingMessage("Id-2", new Dictionary<string, string>(), bytes);
            var outgoingMessage3 = new OutgoingMessage("Id-3", new Dictionary<string, string>(), bytes);
            var outgoingMessage4 = new OutgoingMessage("Id-4", new Dictionary<string, string>(), bytes);
            var dispatchOptions1 = new DispatchOptions(new DirectToTargetDestination("MyQueue"), null, Enumerable.Empty<DeliveryConstraint>());
            var dispatchOptions2 = new DispatchOptions(new DirectToTargetDestination("MyQueue2"), null, Enumerable.Empty<DeliveryConstraint>());

            //// perform the test
            var dispatcher = new Dispatcher(router);

            //validate
            Assert.That(async () => await dispatcher.Dispatch(new[]
            {
                new TransportOperation(outgoingMessage1, dispatchOptions1), new TransportOperation(outgoingMessage2, dispatchOptions1), //batch #1
                new TransportOperation(outgoingMessage3, dispatchOptions2), new TransportOperation(outgoingMessage4, dispatchOptions2), //batch #2
            }), Throws.Exception.TypeOf<AggregateException>());

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }


        class FakeAddressingStrategy : IAddressingStrategy
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