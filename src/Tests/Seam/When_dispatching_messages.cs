namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using DeliveryConstraints;
    using NServiceBus.Extensibility;
    using NServiceBus.ObjectBuilder;
    using Routing;
    using Settings;
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
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingMessageRouter(new FakeTopologySectionManager(), new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings), clientLifecycleManager, settings);

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = namespaceManagerLifeCycleManager.Get(AzureServiceBusConnectionString.Value);
            await creator.Create("myqueue", namespaceManager);
            await creator.Create("myqueue2", namespaceManager);

            var bytes = Encoding.UTF8.GetBytes("Whatever");
            var outgoingMessage1 = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);
            var outgoingMessage2 = new OutgoingMessage("Id-2", new Dictionary<string, string>(), bytes);
            var outgoingMessage3 = new OutgoingMessage("Id-3", new Dictionary<string, string>(), bytes);
            var outgoingMessage4 = new OutgoingMessage("Id-4", new Dictionary<string, string>(), bytes);
            var dispatchOptions1 = new DispatchOptions(new UnicastAddressTag("MyQueue"), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>());
            var dispatchOptions2 = new DispatchOptions(new UnicastAddressTag("MyQueue2"), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>());

            //// perform the test
            var dispatcher = new Dispatcher(router, settings);
            await dispatcher.Dispatch(new []
            {
                new TransportOperation(outgoingMessage1, dispatchOptions1), new TransportOperation(outgoingMessage2, dispatchOptions1), //batch #1
                new TransportOperation(outgoingMessage3, dispatchOptions2), new TransportOperation(outgoingMessage4, dispatchOptions2), //batch #2
            }, new ContextBag());

            //validate
            var queue = await namespaceManager.GetQueue("myqueue");
            Assert.IsTrue(queue.MessageCount == 2, $"'myqueue' was expected to have 2 message, but it didn't ({queue.MessageCount} found)");
            queue = await namespaceManager.GetQueue("myqueue2");
            Assert.IsTrue(queue.MessageCount == 2, $"'myqueue2' was expected to have 2 message, but it didn't ({queue.MessageCount} found)");

            //cleanup 
            await namespaceManager.DeleteQueue("myqueue");
            await namespaceManager.DeleteQueue("myqueue2");
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
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingMessageRouter(new FakeTopologySectionManager(), new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings), clientLifecycleManager, settings);

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = namespaceManagerLifeCycleManager.Get(AzureServiceBusConnectionString.Value);
            await creator.Create("myqueue", namespaceManager);

            var bytes = Encoding.UTF8.GetBytes("Whatever");
            var outgoingMessage1 = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);
            var outgoingMessage2 = new OutgoingMessage("Id-2", new Dictionary<string, string>(), bytes);
            var outgoingMessage3 = new OutgoingMessage("Id-3", new Dictionary<string, string>(), bytes);
            var outgoingMessage4 = new OutgoingMessage("Id-4", new Dictionary<string, string>(), bytes);
            var dispatchOptions1 = new DispatchOptions(new UnicastAddressTag("MyQueue"), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>());
            var dispatchOptions2 = new DispatchOptions(new UnicastAddressTag("MyQueue2"), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>());

            //// perform the test
            var dispatcher = new Dispatcher(router, settings);

            //validate
            Assert.That(async () => await dispatcher.Dispatch(new[]
            {
                new TransportOperation(outgoingMessage1, dispatchOptions1), new TransportOperation(outgoingMessage2, dispatchOptions1), //batch #1
                new TransportOperation(outgoingMessage3, dispatchOptions2), new TransportOperation(outgoingMessage4, dispatchOptions2), //batch #2
            }, new ContextBag()), Throws.Exception);

            //cleanup 
            await namespaceManager.DeleteQueue("myqueue");
        }


        class FakeTopologySectionManager : ITopologySectionManager
        {
            public void InitializeSettings(SettingsHolder settings)
            {
                throw new NotImplementedException();
            }

            public void InitializeContainer(IConfigureComponents container, ITransportPartsContainer transportPartsContainer)
            {
                throw new NotImplementedException();
            }

            public TopologySection DetermineReceiveResources()
            {
                throw new NotImplementedException();
            }

            public TopologySection DetermineResourcesToCreate()
            {
                throw new NotImplementedException();
            }
            
            public TopologySection DeterminePublishDestination(Type eventType)
            {
                throw new NotImplementedException();
            }

            public TopologySection DetermineSendDestination(string destination)
            {
                return new TopologySection
                {
                    Entities = new[]
                    {
                        new EntityInfo
                        {
                            Path = destination,
                            Namespace = new NamespaceInfo(AzureServiceBusConnectionString.Value, NamespaceMode.Active)
                        }
                    }
                };
            }

            public TopologySection DetermineResourcesToSubscribeTo(Type eventType)
            {
                throw new NotImplementedException();
            }

            public TopologySection DetermineResourcesToUnsubscribeFrom(Type eventtype)
            {
                throw new NotImplementedException();
            }
        }
    }
}