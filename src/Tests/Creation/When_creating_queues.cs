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

            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            //make sure there is no leftover from previous test
            namespaceManager.DeleteQueue("myqueue");

            settings.Set("Transport.CreateQueues", false);
            
            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            Assert.IsFalse(namespaceManager.QueueExists("myqueue"));
        }

        [Test]
        public void Uses_queue_description_when_provided_by_user()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            var descriptionToUse = new QueueDescription("myqueue");
            
            extensions.Topology().Resources().Queues().DescriptionFactory((name, s) => descriptionToUse);

            var creator = new AzureServiceBusQueueCreator(settings);

            var description = creator.Create("myqueue", namespaceManager);

            Assert.IsTrue(namespaceManager.QueueExists("myqueue"));
            Assert.AreEqual(descriptionToUse, description);

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public void Properly_sets_ForwardTo_on_the_created_entity()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            // forwarding queue needs to exist before you can use it as a forwarding target
            // needs to be created with different settings as it cannot forward to itself obviously
            var originalsettings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var originalcreator = new AzureServiceBusQueueCreator(originalsettings);
            originalcreator.Create("myotherqueue", namespaceManager);

            // actual test
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().ForwardTo("myotherqueue");

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");

            Assert.IsTrue(real.ForwardTo.EndsWith("myotherqueue"));

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
            namespaceManager.DeleteQueue("myotherqueue");
        }

        [Test]
        public void Properly_sets_ForwardTo_on_the_created_entity_that_qualifies_condition()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);
           
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().ForwardTo(name => name == "myqueue", "myotherqueue");

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myotherqueue", namespaceManager);
            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");
            var forwardReal = namespaceManager.GetQueue("myotherqueue");

            Assert.IsTrue(real.ForwardTo.EndsWith("myotherqueue"));
            Assert.IsTrue(string.IsNullOrEmpty(forwardReal.ForwardTo));

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
            namespaceManager.DeleteQueue("myotherqueue");
        }

        [Test]
        public void Properly_sets_ForwardDeadLetteredMessagesTo_on_the_created_entity()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            // forwarding queue needs to exist before you can use it as a forwarding target
            // needs to be created with different settings as it cannot forward to itself obviously
            var originalsettings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var originalcreator = new AzureServiceBusQueueCreator(originalsettings);
            originalcreator.Create("myotherqueue", namespaceManager);

            // actual test
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().ForwardDeadLetteredMessagesTo("myotherqueue");

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");

            Assert.IsTrue(real.ForwardDeadLetteredMessagesTo.EndsWith("myotherqueue"));

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
            namespaceManager.DeleteQueue("myotherqueue");
        }

        [Test]
        public void Properly_sets_ForwardDeadLetteredMessagesTo_on_the_created_entity_that_qualifies_condition()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().ForwardDeadLetteredMessagesTo(name => name == "myqueue", "myotherqueue");

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myotherqueue", namespaceManager);
            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");
            var forwardReal = namespaceManager.GetQueue("myotherqueue");

            Assert.IsTrue(real.ForwardDeadLetteredMessagesTo.EndsWith("myotherqueue"));
            Assert.IsTrue(string.IsNullOrEmpty(forwardReal.ForwardDeadLetteredMessagesTo));

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
            namespaceManager.DeleteQueue("myotherqueue");
        }

        [Test]
        public void Properly_sets_EnableExpress_on_the_created_entity()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().EnableExpress(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");

            Assert.IsTrue(real.EnableExpress);

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
        }

        [Test, Ignore("There seems to be a server side bug in ASB that doesn't set the actual value")]
        public void Properly_sets_AutoDeleteOnIdle_on_the_created_entity()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().AutoDeleteOnIdle(TimeSpan.FromDays(1));

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");

            Assert.AreEqual(TimeSpan.FromDays(1), real.AutoDeleteOnIdle);

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public void Properly_sets_EnablePartitioning_on_the_created_entity()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            //make sure any previously created queues with this name are removed as the EnablePartitioning cannot be updated
            namespaceManager.DeleteQueue("myqueue");

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().EnablePartitioning(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");

            Assert.IsTrue(real.EnablePartitioning);

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public void Properly_sets_EnableBatchedOperations_on_the_created_entity()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().EnableBatchedOperations(false);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");

            Assert.IsFalse(real.EnableBatchedOperations);

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public void Properly_sets_MaxDeliveryCount_on_the_created_entity()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().MaxDeliveryCount(10);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");

            Assert.AreEqual(10, real.MaxDeliveryCount);

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public void Properly_sets_DuplicateDetectionHistoryTimeWindow_on_the_created_entity()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().DuplicateDetectionHistoryTimeWindow(TimeSpan.FromMinutes(20));

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");

            Assert.AreEqual(TimeSpan.FromMinutes(20), real.DuplicateDetectionHistoryTimeWindow);

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public void Properly_sets_EnableDeadLetteringOnMessageExpiration_on_the_created_entity()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().EnableDeadLetteringOnMessageExpiration(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");

            Assert.IsTrue(real.EnableDeadLetteringOnMessageExpiration);

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
        }

        public void Properly_sets_DefaultMessageTimeToLive_on_the_created_entity()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().DefaultMessageTimeToLive(TimeSpan.FromDays(1));

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");

            Assert.AreEqual(TimeSpan.FromDays(1), real.DefaultMessageTimeToLive);

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
        }

        public void Properly_sets_RequiresSession_on_the_created_entity()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            //make sure any existing queue is cleaned up as the RequiresSession property cannot be changed on existing queues
            namespaceManager.DeleteQueue("myqueue");

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().RequiresSession(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");

            Assert.IsTrue(real.RequiresSession);

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
        }

        public void Properly_sets_RequiresDuplicateDetection_on_the_created_entity()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().RequiresDuplicateDetection(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");

            Assert.IsTrue(real.RequiresDuplicateDetection);

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
        }

        public void Properly_sets_MaxSizeInMegabytes_on_the_created_entity()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().MaxSizeInMegabytes(3072);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");

            Assert.AreEqual(3072, real.MaxSizeInMegabytes);

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
        }

        public void Properly_sets_LockDuration_on_the_created_entity()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().LockDuration(TimeSpan.FromMinutes(5));

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");

            Assert.AreEqual(TimeSpan.FromMinutes(5), real.LockDuration);

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
        }

        public void Properly_sets_SupportOrdering_on_the_created_entity()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().SupportOrdering(true);

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");

            Assert.IsTrue(real.SupportOrdering);

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
        }
    }



}