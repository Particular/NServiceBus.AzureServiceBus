namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using DeliveryConstraints;
    using NServiceBus.Extensibility;
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
            var router = new DefaultOutgoingBatchRouter(new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings), clientLifecycleManager, settings, new ThrowOnOversizedBrokeredMessages());

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = namespaceManagerLifeCycleManager.Get(AzureServiceBusConnectionString.Value);
            await creator.Create("myqueue", namespaceManager);
            await creator.Create("myqueue2", namespaceManager);

            // perform the test
            var dispatcher = new Dispatcher(router, new FakeBatcher());
            await dispatcher.Dispatch(new TransportOperations(new List<MulticastTransportOperation>(), new List<UnicastTransportOperation>()), new ContextBag());

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
            var router = new DefaultOutgoingBatchRouter(new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings), clientLifecycleManager, settings, new ThrowOnOversizedBrokeredMessages());

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = namespaceManagerLifeCycleManager.Get(AzureServiceBusConnectionString.Value);
            await creator.Create("myqueue", namespaceManager);

            // perform the test
            var dispatcher = new Dispatcher(router, new FakeBatcher());

            // validate
            Assert.That(async () => await dispatcher.Dispatch(new TransportOperations(new List<MulticastTransportOperation>(), new List<UnicastTransportOperation>()), new ContextBag()), Throws.Exception);

            //cleanup 
            await namespaceManager.DeleteQueue("myqueue");
        }


        class FakeBatcher : IBatcher
        {
            public IList<Batch> ToBatches(TransportOperations operations)
            {
                // we don't care about incoming operations as we'll fake batcher and return pre-canned batches

                var @namespace = new NamespaceInfo(AzureServiceBusConnectionString.Value, NamespaceMode.Active);

                var bytes = Encoding.UTF8.GetBytes("Whatever");


                var batch1 = new Batch
                {
                    Destinations = new TopologySection
                    {
                        Entities = new List<EntityInfo>
                        {
                            new EntityInfo
                            {
                                Namespace = @namespace,
                                Path = "MyQueue",
                                Type = EntityType.Queue
                            }
                        },
                        Namespaces = new List<NamespaceInfo>
                        {
                            @namespace
                        }
                    },
                    RequiredDispatchConsistency = DispatchConsistency.Default,
                    Operations = new List<BatchedOperation>
                    {
                        new BatchedOperation
                        {
                            Message = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes),
                            DeliveryConstraints = new List<DeliveryConstraint>()
                        },
                        new BatchedOperation
                        {
                            Message = new OutgoingMessage("Id-2", new Dictionary<string, string>(), bytes),
                            DeliveryConstraints = new List<DeliveryConstraint>()
                        },
                    }
                };

                var batch2 = new Batch
                {
                    Destinations = new TopologySection
                    {
                        Entities = new List<EntityInfo>
                        {
                            new EntityInfo
                            {
                                Namespace = @namespace,
                                Path = "MyQueue2",
                                Type = EntityType.Queue
                            }
                        },
                        Namespaces = new List<NamespaceInfo>
                        {
                            @namespace
                        }
                    },
                    RequiredDispatchConsistency = DispatchConsistency.Default,
                    Operations = new List<BatchedOperation>
                    {
                        new BatchedOperation
                        {
                            Message = new OutgoingMessage("Id-3", new Dictionary<string, string>(), bytes),
                            DeliveryConstraints = new List<DeliveryConstraint>()
                        },
                        new BatchedOperation
                        {
                            Message = new OutgoingMessage("Id-4", new Dictionary<string, string>(), bytes),
                            DeliveryConstraints = new List<DeliveryConstraint>()
                        },
                    }
                };

                return new List<Batch>
                {
                    batch1,
                    batch2
                };
            }
        }
    }
}