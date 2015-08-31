namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_queues
    {
        [Test]
        public async Task Does_not_create_queues_when_createqueues_is_set_to_false()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            //make sure there is no leftover from previous test
            await namespaceManager.DeleteQueueAsync("myqueue");

            settings.Set("Transport.CreateQueues", false);
            
            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myqueue", namespaceManager);

            Assert.IsFalse(await namespaceManager.QueueExistsAsync("myqueue"));
        }

        [Test]
        public async Task Uses_queue_description_when_provided_by_user()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var descriptionToUse = new QueueDescription("myqueue");
            
            extensions.Topology().Resources().Queues().DescriptionFactory((name, s) => descriptionToUse);

            var creator = new AzureServiceBusQueueCreator(settings);

            var description = await creator.CreateAsync("myqueue", namespaceManager);

            Assert.IsTrue(await namespaceManager.QueueExistsAsync("myqueue"));
            Assert.AreEqual(descriptionToUse, description);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }

        [Test]
        public async Task Properly_sets_ForwardTo_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            // forwarding queue needs to exist before you can use it as a forwarding target
            // needs to be created with different settings as it cannot forward to itself obviously
            var originalsettings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var originalcreator = new AzureServiceBusQueueCreator(originalsettings);
            await originalcreator.CreateAsync("myotherqueue", namespaceManager);

            // actual test
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().ForwardTo("myotherqueue");

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");

            Assert.IsTrue(real.ForwardTo.EndsWith("myotherqueue"));

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
            await namespaceManager.DeleteQueueAsync("myotherqueue");
        }

        [Test]
        public async Task Properly_sets_ForwardTo_on_the_created_entity_that_qualifies_condition()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
           
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().ForwardTo(name => name == "myqueue", "myotherqueue");

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myotherqueue", namespaceManager);
            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");
            var forwardReal = await namespaceManager.GetQueueAsync("myotherqueue");

            Assert.IsTrue(real.ForwardTo.EndsWith("myotherqueue"));
            Assert.IsTrue(string.IsNullOrEmpty(forwardReal.ForwardTo));

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
            await namespaceManager.DeleteQueueAsync("myotherqueue");
        }

        [Test]
        public async Task Properly_sets_ForwardDeadLetteredMessagesTo_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            // forwarding queue needs to exist before you can use it as a forwarding target
            // needs to be created with different settings as it cannot forward to itself obviously
            var originalsettings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var originalcreator = new AzureServiceBusQueueCreator(originalsettings);
            await originalcreator.CreateAsync("myotherqueue", namespaceManager);

            // actual test
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().ForwardDeadLetteredMessagesTo("myotherqueue");

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");

            Assert.IsTrue(real.ForwardDeadLetteredMessagesTo.EndsWith("myotherqueue"));

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
            await namespaceManager.DeleteQueueAsync("myotherqueue");
        }

        [Test]
        public async Task Properly_sets_ForwardDeadLetteredMessagesTo_on_the_created_entity_that_qualifies_condition()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().ForwardDeadLetteredMessagesTo(name => name == "myqueue", "myotherqueue");

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myotherqueue", namespaceManager);
            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");
            var forwardReal = await namespaceManager.GetQueueAsync("myotherqueue");

            Assert.IsTrue(real.ForwardDeadLetteredMessagesTo.EndsWith("myotherqueue"));
            Assert.IsTrue(string.IsNullOrEmpty(forwardReal.ForwardDeadLetteredMessagesTo));

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
            await namespaceManager.DeleteQueueAsync("myotherqueue");
        }

        [Test]
        public async Task Properly_sets_EnableExpress_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().EnableExpress(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");

            Assert.IsTrue(real.EnableExpress);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }

        [Test]
        public async Task Properly_sets_AutoDeleteOnIdle_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().AutoDeleteOnIdle(TimeSpan.FromDays(1));

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");

            Assert.AreEqual(TimeSpan.FromDays(1), real.AutoDeleteOnIdle);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }

        [Test]
        public async Task Properly_sets_EnablePartitioning_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            //make sure any previously created queues with this name are removed as the EnablePartitioning cannot be updated
            await namespaceManager.DeleteQueueAsync("myqueue");

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().EnablePartitioning(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");

            Assert.IsTrue(real.EnablePartitioning);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }

        [Test]
        public async Task Properly_sets_EnableBatchedOperations_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().EnableBatchedOperations(false);

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");

            Assert.IsFalse(real.EnableBatchedOperations);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }

        [Test]
        public async Task Properly_sets_MaxDeliveryCount_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().MaxDeliveryCount(10);

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");

            Assert.AreEqual(10, real.MaxDeliveryCount);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }

        [Test]
        public async Task Properly_sets_DuplicateDetectionHistoryTimeWindow_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().DuplicateDetectionHistoryTimeWindow(TimeSpan.FromMinutes(20));

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");

            Assert.AreEqual(TimeSpan.FromMinutes(20), real.DuplicateDetectionHistoryTimeWindow);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }

        [Test]
        public async Task Properly_sets_EnableDeadLetteringOnMessageExpiration_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().EnableDeadLetteringOnMessageExpiration(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");

            Assert.IsTrue(real.EnableDeadLetteringOnMessageExpiration);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }

        public async Task Properly_sets_DefaultMessageTimeToLive_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().DefaultMessageTimeToLive(TimeSpan.FromDays(1));

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");

            Assert.AreEqual(TimeSpan.FromDays(1), real.DefaultMessageTimeToLive);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }

        public async Task Properly_sets_RequiresSession_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            //make sure any existing queue is cleaned up as the RequiresSession property cannot be changed on existing queues
            await namespaceManager.DeleteQueueAsync("myqueue");

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().RequiresSession(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");

            Assert.IsTrue(real.RequiresSession);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }

        public async Task Properly_sets_RequiresDuplicateDetection_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().RequiresDuplicateDetection(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");

            Assert.IsTrue(real.RequiresDuplicateDetection);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }

        public async Task Properly_sets_MaxSizeInMegabytes_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().MaxSizeInMegabytes(3072);

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");

            Assert.AreEqual(3072, real.MaxSizeInMegabytes);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }

        public async Task Properly_sets_LockDuration_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().LockDuration(TimeSpan.FromMinutes(5));

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");

            Assert.AreEqual(TimeSpan.FromMinutes(5), real.LockDuration);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }

        public async Task Properly_sets_SupportOrdering_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().SupportOrdering(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            await creator.CreateAsync("myqueue", namespaceManager);

            var real = await namespaceManager.GetQueueAsync("myqueue");

            Assert.IsTrue(real.SupportOrdering);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }
    }



}