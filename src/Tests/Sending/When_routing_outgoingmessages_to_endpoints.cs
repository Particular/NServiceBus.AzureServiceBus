namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.AzureServiceBus.Topology.MetaModel;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.Settings;
    using NServiceBus.Transports;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_routing_outgoingmessages_to_endpoints
    {
        [Test]
        public async Task Can_route_an_outgoing_single_message()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingBatchRouter(new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new PassThroughMapper()), clientLifecycleManager, settings, new ThrowOnOversizedBrokeredMessages());

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = namespaceManagerLifeCycleManager.Get("namespace");
            await creator.Create("myqueue", namespaceManager);

            // setup the batch
            var @namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value, NamespaceMode.Active);
            var bytes = Encoding.UTF8.GetBytes("Whatever");
            var batch = new Batch
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
                    Namespaces = new List<RuntimeNamespaceInfo>
                        {
                            @namespace
                        }
                },
                RequiredDispatchConsistency = DispatchConsistency.Default,
                Operations = new List<BatchedOperation>
                    {
                        new BatchedOperation
                        {
                            Message = new OutgoingMessage("SomeId", new Dictionary<string, string>(), bytes),
                            DeliveryConstraints = new List<DeliveryConstraint>()
                        },
                    }
            };

            // perform the test
            await router.RouteBatch(batch, null);

            //validate
            var queue = await namespaceManager.GetQueue("myqueue");
            Assert.IsTrue(queue.MessageCount > 0, "expected to have messages in the queue, but there were no messages");

            //cleanup 
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Can_route_an_outgoing_batch_of_messages()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingBatchRouter(new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new PassThroughMapper()), clientLifecycleManager, settings, new ThrowOnOversizedBrokeredMessages());

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = namespaceManagerLifeCycleManager.Get("namespace");
            await creator.Create("myqueue", namespaceManager);

            // setup the batch
            var @namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value, NamespaceMode.Active);
            var bytes = Encoding.UTF8.GetBytes("Whatever");
            var batch = new Batch
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
                    Namespaces = new List<RuntimeNamespaceInfo>
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
                        }
                    }
            };

            // perform the test
            await router.RouteBatch(batch, null);

            //validate
            var queue = await namespaceManager.GetQueue("myqueue");
            Assert.IsTrue(queue.MessageCount == 2);

            //cleanup 
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Can_route_a_batch_of_large_messages_that_total_size_exceeds_256KB()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingBatchRouter(new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new PassThroughMapper()), clientLifecycleManager, settings, new ThrowOnOversizedBrokeredMessages());

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = namespaceManagerLifeCycleManager.Get("namespace");
            await creator.Create("myqueue", namespaceManager);

            // setup the batch
            var @namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value, NamespaceMode.Active);
            var bytes = Enumerable.Range(0, 220 * 1024).Select(x => (byte)(x % 256)).ToArray();
            var batch = new Batch
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
                    Namespaces = new List<RuntimeNamespaceInfo>
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
                        }
                    }
            };

            // perform the test
            await router.RouteBatch(batch, null);

            //validate
            var queue = await namespaceManager.GetQueue("myqueue");
            Assert.IsTrue(queue.MessageCount == 2);

            //cleanup 
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Should_throw_exception_for_a_message_that_exceeds_maximum_size()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingBatchRouter(new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new PassThroughMapper()), clientLifecycleManager, settings, new ThrowOnOversizedBrokeredMessages());
            
            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = namespaceManagerLifeCycleManager.Get("namespace");
            await creator.Create("myqueue", namespaceManager);

            // setup the batch
            var @namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value, NamespaceMode.Active);
            var bytes = Enumerable.Range(0, settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximumMessageSizeInKilobytes) * 1024).Select(x => (byte)(x % 256)).ToArray();

            var batch = new Batch
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
                    Namespaces = new List<RuntimeNamespaceInfo>
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
                        }
                    }
            };

            // perform the test
            Assert.That(async () => await router.RouteBatch(batch, null), Throws.Exception.TypeOf<MessageTooLargeException>());

            //cleanup 
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Should_invoke_oversized_brokered_message_handler_for_a_message_that_exceeds_maximum_size()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value);

            var oversizedHandler = new MyOversizedBrokeredMessageHandler();
            
            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingBatchRouter(new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new PassThroughMapper()), clientLifecycleManager, settings, oversizedHandler);

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = namespaceManagerLifeCycleManager.Get("namespace");
            await creator.Create("myqueue", namespaceManager);

            // setup the batch
            var @namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value, NamespaceMode.Active);
            var bytes = Enumerable.Range(0, settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximumMessageSizeInKilobytes) * 1024).Select(x => (byte)(x % 256)).ToArray();

            var batch = new Batch
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
                    Namespaces = new List<RuntimeNamespaceInfo>
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
                        }
                    }
            };

            // perform the test
            await router.RouteBatch(batch, null);

            // validate
            Assert.True(oversizedHandler.Invoked);

            //cleanup 
            await namespaceManager.DeleteQueue("myqueue");
        }

        private class MyOversizedBrokeredMessageHandler : IHandleOversizedBrokeredMessages
        {
            public bool Invoked { get; set; }

            public Task Handle(BrokeredMessage brokeredMessage)
            {
                Invoked = true;
                return TaskEx.Completed;
            }
        }

        private class PassThroughMapper : ICanMapNamespaceNameToConnectionString
        {
            public EntityAddress Map(EntityAddress value)
            {
                return value;
            }
        }
    }
    
}