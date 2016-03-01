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

        [Test]
        public async Task Can_route_via_fallback_namespace()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespacesDefinition.Add("primary", AzureServiceBusConnectionString.Value);
            namespacesDefinition.Add("fallback", AzureServiceBusConnectionString.Fallback);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingBatchRouter(new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new PassThroughMapper()), clientLifecycleManager, settings, new ThrowOnOversizedBrokeredMessages());

            // create the fallback queue (but not the queue in the primary to emulate that it is down)
            var creator = new AzureServiceBusQueueCreator(settings);
            var fallbackNamespaceManager = namespaceManagerLifeCycleManager.Get("fallback");
            await creator.Create("myqueue", fallbackNamespaceManager);

            // setup the batch
            var @namespace = new RuntimeNamespaceInfo("primary", AzureServiceBusConnectionString.Value, NamespaceMode.Active);
            var fallback = new RuntimeNamespaceInfo("fallback", AzureServiceBusConnectionString.Fallback, NamespaceMode.Passive);
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
                            @namespace,
                            fallback
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
            var queue = await fallbackNamespaceManager.GetQueue("myqueue");
            Assert.IsTrue(queue.MessageCount > 0, "expected to have messages in the queue, but there were no messages");

            //cleanup 
            await fallbackNamespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Should_invoke_non_throwing_oversized_brokered_message_handler_for_a_message_that_exceeds_maximum_size_only_once_even_if_fallback_namespace_is_set()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespacesDefinition.Add("primary", AzureServiceBusConnectionString.Value);
            namespacesDefinition.Add("fallback", AzureServiceBusConnectionString.Fallback);

            var oversizedHandler = new MyOversizedBrokeredMessageHandler();

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingBatchRouter(new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new PassThroughMapper()), clientLifecycleManager, settings, oversizedHandler);

            // create the queue & fallback queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = namespaceManagerLifeCycleManager.Get("primary");
            await creator.Create("myqueue", namespaceManager);

            var fallbackNamespaceManager = namespaceManagerLifeCycleManager.Get("fallback");
            await creator.Create("myqueue", fallbackNamespaceManager);

            // setup the batch
            var @namespace = new RuntimeNamespaceInfo("primary", AzureServiceBusConnectionString.Value, NamespaceMode.Active);
            var @fallback = new RuntimeNamespaceInfo("fallback", AzureServiceBusConnectionString.Value, NamespaceMode.Passive);
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
                                Path = "myqueue",
                                Type = EntityType.Queue
                            }
                        },
                    Namespaces = new List<RuntimeNamespaceInfo>
                        {
                            @namespace,
                            @fallback
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
            Assert.True(oversizedHandler.InvocationCount == 1);

            //cleanup 
            await fallbackNamespaceManager.DeleteQueue("myqueue");
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Should_throw_exception_for_a_message_that_exceeds_maximum_size_and_not_handle_fallback_on_message_too_large_Exception()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespacesDefinition.Add("primary", AzureServiceBusConnectionString.Value);
            namespacesDefinition.Add("fallback", AzureServiceBusConnectionString.Fallback);

            var oversizedHandler = new MyThrowingOversizedBrokeredMessageHandler();

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
            var namespaceManager = namespaceManagerLifeCycleManager.Get("primary");
            await creator.Create("myqueue", namespaceManager);

            // setup the batch
            var @namespace = new RuntimeNamespaceInfo("primary", AzureServiceBusConnectionString.Value, NamespaceMode.Active);
            var fallback = new RuntimeNamespaceInfo("fallback", AzureServiceBusConnectionString.Value, NamespaceMode.Passive);
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
                            @namespace,
                            fallback
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
            Assert.True(oversizedHandler.InvocationCount == 1);

            //cleanup 
            await namespaceManager.DeleteQueue("myqueue");
        }

        public class MyOversizedBrokeredMessageHandler : IHandleOversizedBrokeredMessages
        {
            public bool Invoked { get; set; }

            public int InvocationCount { get; set; }

            public Task Handle(BrokeredMessage brokeredMessage)
            {
                InvocationCount++;
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

        public class MyThrowingOversizedBrokeredMessageHandler : IHandleOversizedBrokeredMessages
        {
            public bool Invoked { get; set; }

            public int InvocationCount { get; set; }

            public Task Handle(BrokeredMessage brokeredMessage)
            {
                InvocationCount++;
                Invoked = true;
                
                throw new MessageTooLargeException();
            }
        }
    }
    
}