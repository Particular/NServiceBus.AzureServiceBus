namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using Settings;
    using NUnit.Framework;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_queues
    {
        [Test]
        public void Should_be_able_to_set_AutoDeleteOnIdle()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var idlePeriod = TimeSpan.FromDays(10);
            extensions.Queues().AutoDeleteOnIdle(idlePeriod);

            Assert.AreEqual(idlePeriod, settings.Get<TopologySettings>().QueueSettings.AutoDeleteOnIdle);
        }

        [Test]
        public void Should_be_able_to_set_DefaultMessageTimeToLive()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var expiryTimespan = TimeSpan.FromDays(1);
            extensions.Queues().DefaultMessageTimeToLive(expiryTimespan);

            Assert.AreEqual(expiryTimespan, settings.Get<TopologySettings>().QueueSettings.DefaultMessageTimeToLive);
        }

        [Test]
        public void Should_be_able_to_set_DuplicateDetectionHistoryTimeWindow()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var dedupTimespan = TimeSpan.FromMinutes(20);
            extensions.Queues().DuplicateDetectionHistoryTimeWindow(dedupTimespan);

            Assert.AreEqual(dedupTimespan, settings.Get<TopologySettings>().QueueSettings.DuplicateDetectionHistoryTimeWindow);
        }

        [Test]
        public void Should_be_able_to_set_EnableBatchedOperations()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().EnableBatchedOperations(true);

            Assert.IsTrue(settings.Get<TopologySettings>().QueueSettings.EnableBatchedOperations);
        }

        [Test]
        public void Should_be_able_to_set_EnableDeadLetteringOnMessageExpiration()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().EnableDeadLetteringOnMessageExpiration(true);

            Assert.IsTrue(settings.Get<TopologySettings>().QueueSettings.EnableDeadLetteringOnMessageExpiration);
        }

        [Test]
        public void Should_be_able_to_set_EnableExpress()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().EnableExpress(true);

            Assert.IsTrue(settings.Get<TopologySettings>().QueueSettings.EnableExpress);
        }

        [Test]
        public void Should_be_able_to_set_EnableExpress_conditionally()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, bool> condition = name => name != "expressqueue";
            extensions.Queues().EnableExpress(condition, true);

            Assert.IsTrue(settings.Get<TopologySettings>().QueueSettings.EnableExpress);
            Assert.AreEqual(condition, settings.Get<TopologySettings>().QueueSettings.EnableExpressCondition);
        }


        [Test]
        public void Should_be_able_to_set_EnablePartitioning()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().EnablePartitioning(true);

            Assert.IsTrue(settings.Get<TopologySettings>().QueueSettings.EnablePartitioning);
        }

        [Test]
        public void Should_be_able_to_set_ForwardDeadLetteredMessagesTo()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().ForwardDeadLetteredMessagesTo("deadletteredmessages");

            Assert.AreEqual("deadletteredmessages", settings.Get<TopologySettings>().QueueSettings.ForwardDeadLetteredMessagesTo);
        }

        [Test]
        public void Should_be_able_to_set_ForwardDeadLetteredMessagesTo_conditionally()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, bool> condition = n => n != "deadletteredmessages";
            extensions.Queues().ForwardDeadLetteredMessagesTo(condition, "deadletteredmessages");

            Assert.AreEqual("deadletteredmessages", settings.Get<TopologySettings>().QueueSettings.ForwardDeadLetteredMessagesTo);
            Assert.AreEqual(condition, settings.Get<TopologySettings>().QueueSettings.ForwardDeadLetteredMessagesToCondition);
        }

        [Test]
        public void Should_be_able_to_set_LockDuration()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var lockDuration = TimeSpan.FromDays(1);
            extensions.Queues().LockDuration(lockDuration);

            Assert.AreEqual(lockDuration, settings.Get<TopologySettings>().QueueSettings.LockDuration);
        }

        [Test]
        public void Should_be_able_to_set_MaxDeliveryCount()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            const int selectedMaxDeliveryCount = 6;
            extensions.Queues().MaxDeliveryCount(selectedMaxDeliveryCount);

            Assert.AreEqual(selectedMaxDeliveryCount, settings.Get<TopologySettings>().QueueSettings.MaxDeliveryCount);
        }

        [Test]
        public void Should_be_able_to_set_MaxSizeInMegabytes()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            const long maxTopicSizeInMB = 2048;
            extensions.Queues().MaxSizeInMegabytes(SizeInMegabytes.Size2048);

            Assert.AreEqual(maxTopicSizeInMB, (long)settings.Get<TopologySettings>().QueueSettings.MaxSizeInMegabytes);
        }

        [Test]
        public void Should_be_able_to_set_RequiresDuplicateDetection()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().RequiresDuplicateDetection(true);

            Assert.True(settings.Get<TopologySettings>().QueueSettings.RequiresDuplicateDetection);
        }

        [Test]
        public void Should_be_able_to_set_SupportOrdering()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().SupportOrdering(true);

            Assert.IsTrue(settings.Get<TopologySettings>().QueueSettings.SupportOrdering);
        }
    }
}