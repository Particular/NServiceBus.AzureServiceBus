namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Sending
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using TestUtils;
    using Transport.AzureServiceBus;
    using DeliveryConstraints;
    using Microsoft.ServiceBus;
    using Settings;
    using Transport;
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
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var NamespaceManagerLifeCycleManagerInternal = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(NamespaceManagerLifeCycleManagerInternal, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingBatchRouter(new DefaultBatchedOperationsToBrokeredMessagesConverter(settings), clientLifecycleManager, settings, new ThrowOnOversizedBrokeredMessages());

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = NamespaceManagerLifeCycleManagerInternal.Get("namespace");
            await creator.Create("myqueue", namespaceManager);

            // setup the batch
            var @namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value);
            var bytes = Encoding.UTF8.GetBytes("Whatever");
            var batch = new BatchInternal
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
                        Message = new OutgoingMessage("SomeId", new Dictionary<string, string>(), bytes),
                        DeliveryConstraints = new List<DeliveryConstraint>()
                    },
                }
            };

            // perform the test
            await router.RouteBatch(batch, null, DispatchConsistency.Default);

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
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var NamespaceManagerLifeCycleManagerInternal = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(NamespaceManagerLifeCycleManagerInternal, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingBatchRouter(new DefaultBatchedOperationsToBrokeredMessagesConverter(settings), clientLifecycleManager, settings, new ThrowOnOversizedBrokeredMessages());

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = NamespaceManagerLifeCycleManagerInternal.Get("namespace");
            await creator.Create("myqueue", namespaceManager);

            // setup the batch
            var @namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value);
            var bytes = Encoding.UTF8.GetBytes("Whatever");
            var batch = new BatchInternal
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
                    }
                }
            };

            // perform the test
            await router.RouteBatch(batch, null, DispatchConsistency.Default);

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
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var NamespaceManagerLifeCycleManagerInternal = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(NamespaceManagerLifeCycleManagerInternal, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingBatchRouter(new DefaultBatchedOperationsToBrokeredMessagesConverter(settings), clientLifecycleManager, settings, new ThrowOnOversizedBrokeredMessages());

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = NamespaceManagerLifeCycleManagerInternal.Get("namespace");
            await creator.Create("myqueue", namespaceManager);

            // setup the batch
            var @namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value);
            var bytes = Enumerable.Range(0, 220*1024).Select(x => (byte) (x%256)).ToArray();
            var batch = new BatchInternal
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
                    }
                }
            };

            // perform the test
            await router.RouteBatch(batch, null, DispatchConsistency.Default);

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
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var NamespaceManagerLifeCycleManagerInternal = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(NamespaceManagerLifeCycleManagerInternal, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingBatchRouter(new DefaultBatchedOperationsToBrokeredMessagesConverter(settings), clientLifecycleManager, settings, new ThrowOnOversizedBrokeredMessages());

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = NamespaceManagerLifeCycleManagerInternal.Get("namespace");
            await creator.Create("myqueue", namespaceManager);

            // setup the batch
            var @namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value);
            var bytes = Enumerable.Range(0, settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximumMessageSizeInKilobytes)*1024).Select(x => (byte) (x%256)).ToArray();

            var batch = new BatchInternal
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
                    }
                }
            };

            // perform the test
            Assert.That(async () => await router.RouteBatch(batch, null, DispatchConsistency.Default), Throws.Exception.TypeOf<MessageTooLargeException>());

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Should_invoke_oversized_brokered_message_handler_for_a_message_that_exceeds_maximum_size()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            var oversizedHandler = new MyOversizedBrokeredMessageHandler();

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var NamespaceManagerLifeCycleManagerInternal = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(NamespaceManagerLifeCycleManagerInternal, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingBatchRouter(new DefaultBatchedOperationsToBrokeredMessagesConverter(settings), clientLifecycleManager, settings, oversizedHandler);

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = NamespaceManagerLifeCycleManagerInternal.Get("namespace");
            await creator.Create("myqueue", namespaceManager);

            // setup the batch
            var @namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value);
            var bytes = Enumerable.Range(0, settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximumMessageSizeInKilobytes)*1024).Select(x => (byte) (x%256)).ToArray();

            var batch = new BatchInternal
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
                    }
                }
            };

            // perform the test
            await router.RouteBatch(batch, null, DispatchConsistency.Default);

            // validate
            Assert.True(oversizedHandler.Invoked);

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }


        [Test]
        public async Task Should_invoke_non_throwing_oversized_brokered_message_handler_for_a_message_that_exceeds_maximum_size_only_once_even_if_fallback_namespace_is_set()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("primary", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);
            namespacesDefinition.Add("fallback", AzureServiceBusConnectionString.Fallback, NamespacePurpose.Partitioning);

            var oversizedHandler = new MyOversizedBrokeredMessageHandler();

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var NamespaceManagerLifeCycleManagerInternal = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(NamespaceManagerLifeCycleManagerInternal, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingBatchRouter(new DefaultBatchedOperationsToBrokeredMessagesConverter(settings), clientLifecycleManager, settings, oversizedHandler);

            // create the queue & fallback queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = NamespaceManagerLifeCycleManagerInternal.Get("primary");
            await creator.Create("myqueue", namespaceManager);

            var fallbackNamespaceManager = NamespaceManagerLifeCycleManagerInternal.Get("fallback");
            await creator.Create("myqueue", fallbackNamespaceManager);

            // setup the batch
            var @namespace = new RuntimeNamespaceInfo("primary", AzureServiceBusConnectionString.Value);
            var @fallback = new RuntimeNamespaceInfo("fallback", AzureServiceBusConnectionString.Value, mode: NamespaceMode.Passive);
            var bytes = Enumerable.Range(0, settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximumMessageSizeInKilobytes)*1024).Select(x => (byte) (x%256)).ToArray();

            var batch = new BatchInternal
            {
                Destinations = new TopologySectionInternal
                {
                    Entities = new List<EntityInfoInternal>
                    {
                            new EntityInfoInternal
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
                Operations = new List<BatchedOperationInternal>
                {
                        new BatchedOperationInternal
                    {
                        Message = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes),
                        DeliveryConstraints = new List<DeliveryConstraint>()
                    }
                }
            };

            // perform the test
            await router.RouteBatch(batch, null, DispatchConsistency.Default);

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
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("primary", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);
            namespacesDefinition.Add("fallback", AzureServiceBusConnectionString.Fallback, NamespacePurpose.Partitioning);

            var oversizedHandler = new MyThrowingOversizedBrokeredMessageHandler();

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var NamespaceManagerLifeCycleManagerInternal = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(NamespaceManagerLifeCycleManagerInternal, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingBatchRouter(new DefaultBatchedOperationsToBrokeredMessagesConverter(settings), clientLifecycleManager, settings, oversizedHandler);

            // create the queue
            var creator = new AzureServiceBusQueueCreator(settings);
            var namespaceManager = NamespaceManagerLifeCycleManagerInternal.Get("primary");
            await creator.Create("myqueue", namespaceManager);

            // setup the batch
            var @namespace = new RuntimeNamespaceInfo("primary", AzureServiceBusConnectionString.Value);
            var fallback = new RuntimeNamespaceInfo("fallback", AzureServiceBusConnectionString.Value, mode: NamespaceMode.Passive);
            var bytes = Enumerable.Range(0, settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximumMessageSizeInKilobytes)*1024).Select(x => (byte) (x%256)).ToArray();

            var batch = new BatchInternal
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
                        @namespace,
                        fallback
                    }
                },
                RequiredDispatchConsistency = DispatchConsistency.Default,
                Operations = new List<BatchedOperationInternal>
                {
                        new BatchedOperationInternal
                    {
                        Message = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes),
                        DeliveryConstraints = new List<DeliveryConstraint>()
                    }
                }
            };

            // perform the test
            Assert.That(async () => await router.RouteBatch(batch, null, DispatchConsistency.Default), Throws.Exception.TypeOf<MessageTooLargeException>());
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

        [Test]
        public async Task Should_route_via_active_namespace_first()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var messageSenderCreator = new FakeICreateMessageSenders();
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingBatchRouter(new DefaultBatchedOperationsToBrokeredMessagesConverter(settings), clientLifecycleManager, settings, new ThrowOnOversizedBrokeredMessages());

            // setup the batch
            var batch = CreateTestBatchTargetingPrimaryAndSecondaryNamespaces();

            // perform the test
            await router.RouteBatch(batch, null, DispatchConsistency.Default);

            Assert.That(messageSenderCreator.Senders["primary"].NumberOfTimeInvoked, Is.EqualTo(1));
            Assert.That(messageSenderCreator.Senders["fallback"].NumberOfTimeInvoked, Is.EqualTo(0));
        }

        [Test]
        public async Task Can_route_via_fallback_namespace()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var messageSenderCreator = new FakeICreateMessageSenders("primary");
            var clientLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var router = new DefaultOutgoingBatchRouter(new DefaultBatchedOperationsToBrokeredMessagesConverter(settings), clientLifecycleManager, settings, new ThrowOnOversizedBrokeredMessages());

            // setup the batch
            var batch = CreateTestBatchTargetingPrimaryAndSecondaryNamespaces();

            // perform the test
            await router.RouteBatch(batch, null, DispatchConsistency.Default);

            Assert.That(messageSenderCreator.Senders["primary"].NumberOfTimeInvoked, Is.EqualTo(0));
            Assert.That(messageSenderCreator.Senders["fallback"].NumberOfTimeInvoked, Is.EqualTo(1));
        }

        static BatchInternal CreateTestBatchTargetingPrimaryAndSecondaryNamespaces()
        {
            var @namespace = new RuntimeNamespaceInfo("primary", AzureServiceBusConnectionString.Value);
            var fallback = new RuntimeNamespaceInfo("fallback", AzureServiceBusConnectionString.Fallback, mode: NamespaceMode.Passive);
            var bytes = Encoding.UTF8.GetBytes("Whatever");
            var batch = new BatchInternal
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
                        },
                        new EntityInfoInternal
                        {
                            Namespace = fallback,
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
                Operations = new List<BatchedOperationInternal>
                {
                    new BatchedOperationInternal
                    {
                        Message = new OutgoingMessage("SomeId", new Dictionary<string, string>(), bytes),
                        DeliveryConstraints = new List<DeliveryConstraint>()
                    },
                }
            };
            return batch;
        }


        class FakeICreateMessageSenders : ICreateMessageSendersInternal
        {
            readonly string failOnNamespaceAlias;
            public Dictionary<string, FakeIMessageSender> Senders = new Dictionary<string, FakeIMessageSender>();

            public FakeICreateMessageSenders(string failOnNamespaceAlias = "")
            {
                this.failOnNamespaceAlias = failOnNamespaceAlias;
            }

            public Task<IMessageSenderInternal> Create(string entitypath, string viaEntityPath, string namespaceName)
            {
                FakeIMessageSender fakeIMessageSender;
                if (!Senders.TryGetValue(namespaceName, out fakeIMessageSender))
                {
                    fakeIMessageSender = new FakeIMessageSender(namespaceName == failOnNamespaceAlias);
                    Senders.Add(namespaceName, fakeIMessageSender);
                }
                return Task.FromResult<IMessageSenderInternal>(fakeIMessageSender);
            }
        }

        class FakeIMessageSender : IMessageSenderInternal
        {
            readonly bool shouldThrowException;
            public int NumberOfTimeInvoked;

            public FakeIMessageSender(bool shouldThrowException)
            {
                this.shouldThrowException = shouldThrowException;
                NumberOfTimeInvoked = 0;
            }

            public bool IsClosed => false;
            public RetryPolicy RetryPolicy  { get; set; }
            public Task Send(BrokeredMessage message)
            {
                if (shouldThrowException)
                {
                    throw new Exception("kaboom");
                }
                NumberOfTimeInvoked++;
                return Task.FromResult(true);
            }

            public Task SendBatch(IEnumerable<BrokeredMessage> messages)
            {
                if (shouldThrowException)
                {
                    throw new Exception("kaboom");
                }
                NumberOfTimeInvoked++;
                return Task.FromResult(true);
            }
        }
    }
}