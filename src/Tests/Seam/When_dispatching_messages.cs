namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using Tests;
    using TestUtils;
    using Transport.AzureServiceBus;
    using DeliveryConstraints;
    using Extensibility;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
    using Transport;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_dispatching_messages
    {
        [Test]
        public async Task Should_dispatch_multiple_batches()
        {
            // cleanup
            await TestUtility.Delete("myqueue", "myqueue2");

            // default settings
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            settings.Set<TopologySettings>(new TopologySettings());
            settings.Set("NServiceBus.Routing.EndpointName", "myqueue");
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceLifecycleManager = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceLifecycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new OutgoingBatchRouter(new BatchedOperationsToBrokeredMessagesConverter(settings), clientLifecycleManager, settings, new ThrowOnOversizedBrokeredMessages());

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings.Get<TopologySettings>().QueueSettings, settings);
            var namespaceManager = namespaceLifecycleManager.Get("namespace");
            await creator.Create("myqueue", namespaceManager);
            await creator.Create("myqueue2", namespaceManager);

            // perform the test
            var dispatcher = new Dispatcher(router, new FakeBatcher());
            await dispatcher.Dispatch(new TransportOperations(), new TransportTransaction(), new ContextBag());

            //validate
            var queue = await namespaceManager.GetQueue("myqueue");
            Assert.IsTrue(queue.MessageCount == 2, $"'myqueue' was expected to have 2 message, but it didn't ({queue.MessageCount} found)");
            queue = await namespaceManager.GetQueue("myqueue2");
            Assert.IsTrue(queue.MessageCount == 2, $"'myqueue2' was expected to have 2 message, but it didn't ({queue.MessageCount} found)");
        }

        [Test]
        public async Task Should_throw_if_at_least_one_batch_fails()
        {
            // cleanup
            await TestUtility.Delete("myqueue", "myqueue2");

            // default settings
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            settings.Set<TopologySettings>(new TopologySettings());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceLifecycleManager = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceLifecycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new OutgoingBatchRouter(new BatchedOperationsToBrokeredMessagesConverter(settings), clientLifecycleManager, settings, new ThrowOnOversizedBrokeredMessages());

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings.Get<TopologySettings>().QueueSettings, settings);
            var namespaceManager = namespaceLifecycleManager.Get("namespace");
            await creator.Create("myqueue", namespaceManager);

            // perform the test
            var dispatcher = new Dispatcher(router, new FakeBatcher());

            // validate
            Assert.ThrowsAsync<MessagingEntityNotFoundException>(async () => await dispatcher.Dispatch(new TransportOperations(), new TransportTransaction(), new ContextBag()));
        }

        class FakeBatcher : IBatcherInternal
        {
            public IList<BatchInternal> ToBatches(TransportOperations operations)
            {
                // we don't care about incoming operations as we'll fake batcher and return pre-canned batches

                var @namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value);

                var bytes = Encoding.UTF8.GetBytes("Whatever");


                var batch1 = new BatchInternal
                {
                    Destinations = new TopologySectionInternal
                    {
                        Entities = new List<EntityInfoInternal>
                        {
                            new EntityInfoInternal
                            {
                                Namespace = @namespace,
                                Path = "MyQueue",
                                Type = EntityType.Queue
                            }
                        },
                        Namespaces = new List<RuntimeNamespaceInfo>
                        {
                            @namespace
                        }
                    },
                    RequiredDispatchConsistency = DispatchConsistency.Default,
                    Operations = new List<BatchedOperationInternal>
                    {
                        new BatchedOperationInternal
                        {
                            Message = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes),
                            DeliveryConstraints = new List<DeliveryConstraint>()
                        },
                        new BatchedOperationInternal
                        {
                            Message = new OutgoingMessage("Id-2", new Dictionary<string, string>(), bytes),
                            DeliveryConstraints = new List<DeliveryConstraint>()
                        },
                    }
                };

                var batch2 = new BatchInternal
                {
                    Destinations = new TopologySectionInternal
                    {
                        Entities = new List<EntityInfoInternal>
                        {
                            new EntityInfoInternal
                            {
                                Namespace = @namespace,
                                Path = "MyQueue2",
                                Type = EntityType.Queue
                            }
                        },
                        Namespaces = new List<RuntimeNamespaceInfo>
                        {
                            @namespace
                        }
                    },
                    RequiredDispatchConsistency = DispatchConsistency.Default,
                    Operations = new List<BatchedOperationInternal>
                    {
                        new BatchedOperationInternal
                        {
                            Message = new OutgoingMessage("Id-3", new Dictionary<string, string>(), bytes),
                            DeliveryConstraints = new List<DeliveryConstraint>()
                        },
                        new BatchedOperationInternal
                        {
                            Message = new OutgoingMessage("Id-4", new Dictionary<string, string>(), bytes),
                            DeliveryConstraints = new List<DeliveryConstraint>()
                        },
                    }
                };

                return new List<BatchInternal>
                {
                    batch1,
                    batch2
                };
            }
        }
    }
}