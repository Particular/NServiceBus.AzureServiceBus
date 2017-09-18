namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Creation
{
    using System;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using TestUtils;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;
    using Routing;
    using Transport;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_queues
    {
        [Test]
        public async Task Uses_queue_description_when_provided_by_user()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var userQueueDescriptionFactoryWasInvoked = false;
            extensions.Queues().DescriptionCustomizer(qd => userQueueDescriptionFactoryWasInvoked = true);

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            Assert.IsTrue(await namespaceManager.QueueExists("myqueue"));
            Assert.IsTrue(userQueueDescriptionFactoryWasInvoked);

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Properly_sets_ForwardDeadLetteredMessagesTo_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            // forwarding queue needs to exist before you can use it as a forwarding target
            // needs to be created with different settings as it cannot forward to itself obviously
            var originalsettings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var originalcreator = new AzureServiceBusQueueCreator(new TopologyQueueSettings(), originalsettings);
            await originalcreator.Create("myotherqueue", namespaceManager);

            // actual test
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().ForwardDeadLetteredMessagesTo("myotherqueue");

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");

            Assert.IsTrue(real.ForwardDeadLetteredMessagesTo.EndsWith("myotherqueue"));

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
            await namespaceManager.DeleteQueue("myotherqueue");
        }

        [Test]
        public async Task Properly_sets_ForwardDeadLetteredMessagesTo_on_the_created_entity_that_qualifies_condition()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);
            await creator.Create("myotherqueue", namespaceManager);

            extensions.Queues().ForwardDeadLetteredMessagesTo(name => name == "myqueue", "myotherqueue");
            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");
            var forwardReal = await namespaceManager.GetQueue("myotherqueue");

            Assert.IsTrue(real.ForwardDeadLetteredMessagesTo.EndsWith("myotherqueue"));
            Assert.IsTrue(string.IsNullOrEmpty(forwardReal.ForwardDeadLetteredMessagesTo));

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
            await namespaceManager.DeleteQueue("myotherqueue");
        }

        [Test]
        public async Task Properly_sets_EnableExpress_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().EnableExpress(true);

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");

            Assert.IsTrue(real.EnableExpress);

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Properly_sets_EnableExpress_on_the_created_entity_that_qualifies_condition()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            //var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().EnableExpress(name => name == "myqueue", true);

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");

            Assert.IsTrue(real.EnableExpress, "Queue should be marked as express, but it wasn't.");

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Properly_sets_AutoDeleteOnIdle_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().AutoDeleteOnIdle(TimeSpan.FromDays(1));

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");

            Assert.AreEqual(TimeSpan.FromDays(1), real.AutoDeleteOnIdle);

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Properly_sets_EnablePartitioning_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            //make sure any previously created queues with this name are removed as the EnablePartitioning cannot be updated
            await namespaceManager.DeleteQueue("myqueue");

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().EnablePartitioning(true);

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");

            Assert.IsTrue(real.EnablePartitioning);

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Properly_sets_EnableBatchedOperations_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().EnableBatchedOperations(false);

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");

            Assert.IsFalse(real.EnableBatchedOperations);

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Should_set_MaxDeliveryCount_to_specified_value_for_immediate_retries_not_enabled()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().MaxDeliveryCount(100);

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");

            Assert.AreEqual(100, real.MaxDeliveryCount);

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Should_set_MaxDeliveryCount_to_number_of_immediate_retries_plus_1_for_non_system_queues()
        {
            const int numberOfImmediateRetries = 3;

            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            settings.Set(WellKnownConfigurationKeys.Core.RecoverabilityNumberOfImmediateRetries, numberOfImmediateRetries);
            var topology = new FakeTopology(settings);

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");

            Assert.That(real.MaxDeliveryCount, Is.EqualTo(numberOfImmediateRetries + 1));

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Should_set_MaxDeliveryCount_to_10_for_system_queues()
        {
            const int numberOfImmediateRetries = 3;

            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            settings.Set(WellKnownConfigurationKeys.Core.RecoverabilityNumberOfImmediateRetries, numberOfImmediateRetries);
            var bindings = new QueueBindings();
            bindings.BindSending("myqueue");
            settings.Set<QueueBindings>(bindings);
            var topology = new FakeTopology(settings);

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");

            Assert.That(real.MaxDeliveryCount, Is.EqualTo(10));

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }


        [Test]
        public async Task Properly_sets_DuplicateDetectionHistoryTimeWindow_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().DuplicateDetectionHistoryTimeWindow(TimeSpan.FromMinutes(20));

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");

            Assert.AreEqual(TimeSpan.FromMinutes(20), real.DuplicateDetectionHistoryTimeWindow);

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Properly_sets_EnableDeadLetteringOnMessageExpiration_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().EnableDeadLetteringOnMessageExpiration(true);

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");

            Assert.IsTrue(real.EnableDeadLetteringOnMessageExpiration);

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Properly_sets_DefaultMessageTimeToLive_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().DefaultMessageTimeToLive(TimeSpan.FromDays(1));

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");

            Assert.AreEqual(TimeSpan.FromDays(1), real.DefaultMessageTimeToLive);

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Properly_sets_RequiresDuplicateDetection_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().RequiresDuplicateDetection(true);

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");

            Assert.IsTrue(real.RequiresDuplicateDetection);

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Properly_sets_MaxSizeInMegabytes_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().MaxSizeInMegabytes(SizeInMegabytes.Size3072);

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");

            Assert.AreEqual(3072, real.MaxSizeInMegabytes);

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Properly_sets_LockDuration_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().LockDuration(TimeSpan.FromMinutes(5));

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");

            Assert.AreEqual(TimeSpan.FromMinutes(5), real.LockDuration);

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Properly_sets_SupportOrdering_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().SupportOrdering(true);

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueue("myqueue");

            Assert.IsTrue(real.SupportOrdering);

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Should_be_able_to_update_an_existing_queue_with_new_property_values()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteQueue("existingqueue").ConfigureAwait(false);
            await namespaceManager.CreateQueue(new QueueDescription("existingqueue"));

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Queues().DescriptionCustomizer(qd =>
            {
                qd.AutoDeleteOnIdle = TimeSpan.FromMinutes(100);
                qd.EnableExpress = true;
            });

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);
            await creator.Create("existingqueue", namespaceManager);

            var queueDescription = await namespaceManager.GetQueue("existingqueue");
            Assert.AreEqual(TimeSpan.FromMinutes(100), queueDescription.AutoDeleteOnIdle);

            //cleanup
            await namespaceManager.DeleteQueue("existingqueue");
        }

        [Test]
        public async Task Should_be_able_to_update_an_existing_queue_with_new_property_values_without_failing_on_readonly_properties()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteQueue("existingqueue").ConfigureAwait(false);
            await namespaceManager.CreateQueue(new QueueDescription("existingqueue")
            {
                LockDuration = TimeSpan.FromSeconds(50),
                MaxSizeInMegabytes = SizeInMegabytes.Size2048,
                RequiresDuplicateDetection = true,
                EnablePartitioning = true,
                RequiresSession = true
            });

            var queueDescription = await namespaceManager.GetQueue("existingqueue");

            // partitioned topics will have a size that is 16x the requested max
            Assert.AreEqual(2048 * 16, queueDescription.MaxSizeInMegabytes);
            Assert.AreEqual(TimeSpan.FromSeconds(50), queueDescription.LockDuration);
            Assert.IsTrue(queueDescription.EnablePartitioning);
            Assert.IsTrue(queueDescription.RequiresDuplicateDetection);

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Queues().DescriptionCustomizer(qd =>
            {
                qd.LockDuration = TimeSpan.FromSeconds(70);
                qd.MaxSizeInMegabytes = SizeInMegabytes.Size3072;
                qd.RequiresDuplicateDetection = false;
                qd.EnablePartitioning = false;
                qd.RequiresSession = false;
            });

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);
            await creator.Create("existingqueue", namespaceManager);

            queueDescription = await namespaceManager.GetQueue("existingqueue");
            Assert.AreEqual(3072 * 16, queueDescription.MaxSizeInMegabytes);
            Assert.AreEqual(TimeSpan.FromSeconds(70), queueDescription.LockDuration);
            Assert.IsTrue(queueDescription.EnablePartitioning);
            Assert.IsTrue(queueDescription.RequiresDuplicateDetection);

            //cleanup
            await namespaceManager.DeleteQueue("existingqueue");
        }

        [Test]
        public async Task Should_not_update_properties_of_an_existing_system_queue()
        {
            var queuePath = "errorQ";

            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteQueue(queuePath).ConfigureAwait(false);
            if (!await namespaceManager.QueueExists(queuePath).ConfigureAwait(false))
            {
                await namespaceManager.CreateQueue(new QueueDescription(queuePath)).ConfigureAwait(false);
            }

            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var topology = new FakeTopology(settings);
            settings.Set(WellKnownConfigurationKeys.Core.RecoverabilityNumberOfImmediateRetries, 2);
            var queueBindings = new QueueBindings();
            queueBindings.BindSending(queuePath);
            settings.Set<QueueBindings>(queueBindings);

            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Queues().MaxDeliveryCount(2);

            var creator = new AzureServiceBusQueueCreator(topology.Settings.QueueSettings, settings);

            await creator.Create(queuePath, namespaceManager);

            var real = await namespaceManager.GetQueue(queuePath);

            Assert.That(real.MaxDeliveryCount, Is.EqualTo(10));

            //cleanup
            await namespaceManager.DeleteQueue(queuePath);
        }

        class FakeTopology : ITopologyInternal
        {
            public FakeTopology(SettingsHolder settings)
            {
                settings.Set<ITopologyInternal>(this);
            }

            public TopologySettings Settings { get; } = new TopologySettings();

            public void Initialize(SettingsHolder settings)
            {
                throw new NotImplementedException();
            }

            public EndpointInstance BindToLocalEndpoint(EndpointInstance instance)
            {
                throw new NotImplementedException();
            }

            public Func<ICreateQueues> GetQueueCreatorFactory()
            {
                throw new NotImplementedException();
            }

            public Func<IPushMessages> GetMessagePumpFactory()
            {
                throw new NotImplementedException();
            }

            public Func<IDispatchMessages> GetDispatcherFactory()
            {
                throw new NotImplementedException();
            }

            public Task<StartupCheckResult> RunPreStartupChecks()
            {
                throw new NotImplementedException();
            }

            public Func<IManageSubscriptions> GetSubscriptionManagerFactory()
            {
                throw new NotImplementedException();
            }

            public OutboundRoutingPolicy GetOutboundRoutingPolicy()
            {
                throw new NotImplementedException();
            }

            public bool HasNativePubSubSupport { get; }
            public bool HasSupportForCentralizedPubSub { get; }

            public Task Stop()
            {
                throw new NotImplementedException();
            }
        }
    }
}