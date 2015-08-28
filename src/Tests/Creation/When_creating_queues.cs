namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
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
        public void Does_not_create_queues_when_createqueues_is_set_to_false()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            //make sure there is no leftover from previous test
            namespaceManager.DeleteQueueAsync("myqueue").Wait();

            settings.Set("Transport.CreateQueues", false);
            
            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myqueue", namespaceManager).Wait();

            Assert.IsFalse(namespaceManager.QueueExistsAsync("myqueue").Result);
        }

        [Test]
        public void Uses_queue_description_when_provided_by_user()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var descriptionToUse = new QueueDescription("myqueue");
            
            extensions.Topology().Resources().Queues().DescriptionFactory((name, s) => descriptionToUse);

            var creator = new AzureServiceBusQueueCreator(settings);

            var description = creator.CreateAsync("myqueue", namespaceManager).Result;

            Assert.IsTrue(namespaceManager.QueueExistsAsync("myqueue").Result);
            Assert.AreEqual(descriptionToUse, description);

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }

        [Test]
        public void Properly_sets_ForwardTo_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            // forwarding queue needs to exist before you can use it as a forwarding target
            // needs to be created with different settings as it cannot forward to itself obviously
            var originalsettings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var originalcreator = new AzureServiceBusQueueCreator(originalsettings);
            originalcreator.CreateAsync("myotherqueue", namespaceManager).Wait();

            // actual test
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().ForwardTo("myotherqueue");

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;

            Assert.IsTrue(real.ForwardTo.EndsWith("myotherqueue"));

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
            namespaceManager.DeleteQueueAsync("myotherqueue").Wait();
        }

        [Test]
        public void Properly_sets_ForwardTo_on_the_created_entity_that_qualifies_condition()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
           
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().ForwardTo(name => name == "myqueue", "myotherqueue");

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myotherqueue", namespaceManager).Wait();
            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;
            var forwardReal = namespaceManager.GetQueueAsync("myotherqueue").Result;

            Assert.IsTrue(real.ForwardTo.EndsWith("myotherqueue"));
            Assert.IsTrue(string.IsNullOrEmpty(forwardReal.ForwardTo));

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
            namespaceManager.DeleteQueueAsync("myotherqueue").Wait();
        }

        [Test]
        public void Properly_sets_ForwardDeadLetteredMessagesTo_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            // forwarding queue needs to exist before you can use it as a forwarding target
            // needs to be created with different settings as it cannot forward to itself obviously
            var originalsettings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var originalcreator = new AzureServiceBusQueueCreator(originalsettings);
            originalcreator.CreateAsync("myotherqueue", namespaceManager).Wait();

            // actual test
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().ForwardDeadLetteredMessagesTo("myotherqueue");

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;

            Assert.IsTrue(real.ForwardDeadLetteredMessagesTo.EndsWith("myotherqueue"));

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
            namespaceManager.DeleteQueueAsync("myotherqueue").Wait();
        }

        [Test]
        public void Properly_sets_ForwardDeadLetteredMessagesTo_on_the_created_entity_that_qualifies_condition()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().ForwardDeadLetteredMessagesTo(name => name == "myqueue", "myotherqueue");

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myotherqueue", namespaceManager).Wait();
            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;
            var forwardReal = namespaceManager.GetQueueAsync("myotherqueue").Result;

            Assert.IsTrue(real.ForwardDeadLetteredMessagesTo.EndsWith("myotherqueue"));
            Assert.IsTrue(string.IsNullOrEmpty(forwardReal.ForwardDeadLetteredMessagesTo));

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
            namespaceManager.DeleteQueueAsync("myotherqueue").Wait();
        }

        [Test]
        public void Properly_sets_EnableExpress_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().EnableExpress(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;

            Assert.IsTrue(real.EnableExpress);

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }

        [Test]
        public void Properly_sets_AutoDeleteOnIdle_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().AutoDeleteOnIdle(TimeSpan.FromDays(1));

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;

            Assert.AreEqual(TimeSpan.FromDays(1), real.AutoDeleteOnIdle);

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }

        [Test]
        public void Properly_sets_EnablePartitioning_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            //make sure any previously created queues with this name are removed as the EnablePartitioning cannot be updated
            namespaceManager.DeleteQueueAsync("myqueue").Wait();

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().EnablePartitioning(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;

            Assert.IsTrue(real.EnablePartitioning);

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }

        [Test]
        public void Properly_sets_EnableBatchedOperations_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().EnableBatchedOperations(false);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;

            Assert.IsFalse(real.EnableBatchedOperations);

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }

        [Test]
        public void Properly_sets_MaxDeliveryCount_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().MaxDeliveryCount(10);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;

            Assert.AreEqual(10, real.MaxDeliveryCount);

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }

        [Test]
        public void Properly_sets_DuplicateDetectionHistoryTimeWindow_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().DuplicateDetectionHistoryTimeWindow(TimeSpan.FromMinutes(20));

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;

            Assert.AreEqual(TimeSpan.FromMinutes(20), real.DuplicateDetectionHistoryTimeWindow);

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }

        [Test]
        public void Properly_sets_EnableDeadLetteringOnMessageExpiration_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().EnableDeadLetteringOnMessageExpiration(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;

            Assert.IsTrue(real.EnableDeadLetteringOnMessageExpiration);

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }

        public void Properly_sets_DefaultMessageTimeToLive_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().DefaultMessageTimeToLive(TimeSpan.FromDays(1));

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;

            Assert.AreEqual(TimeSpan.FromDays(1), real.DefaultMessageTimeToLive);

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }

        public void Properly_sets_RequiresSession_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            //make sure any existing queue is cleaned up as the RequiresSession property cannot be changed on existing queues
            namespaceManager.DeleteQueueAsync("myqueue").Wait();

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().RequiresSession(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;

            Assert.IsTrue(real.RequiresSession);

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }

        public void Properly_sets_RequiresDuplicateDetection_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().RequiresDuplicateDetection(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;

            Assert.IsTrue(real.RequiresDuplicateDetection);

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }

        public void Properly_sets_MaxSizeInMegabytes_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().MaxSizeInMegabytes(3072);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;

            Assert.AreEqual(3072, real.MaxSizeInMegabytes);

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }

        public void Properly_sets_LockDuration_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().LockDuration(TimeSpan.FromMinutes(5));

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;

            Assert.AreEqual(TimeSpan.FromMinutes(5), real.LockDuration);

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }

        public void Properly_sets_SupportOrdering_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().SupportOrdering(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var real = namespaceManager.GetQueueAsync("myqueue").Result;

            Assert.IsTrue(real.SupportOrdering);

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }
    }



}