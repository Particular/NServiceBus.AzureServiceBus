namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_queues
    {
        [Test]
        public void Should_be_able_to_set_AutoDeleteOnIdle()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var idlePeriod = TimeSpan.FromDays(10);
            extensions.Queues().AutoDeleteOnIdle(idlePeriod);

            Assert.AreEqual(idlePeriod, topology.Settings.QueueSettings.AutoDeleteOnIdle);
        }

        [Test]
        public void Should_be_able_to_set_DefaultMessageTimeToLive()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var expiryTimespan = TimeSpan.FromDays(1);
            extensions.Queues().DefaultMessageTimeToLive(expiryTimespan);

            Assert.AreEqual(expiryTimespan, topology.Settings.QueueSettings.DefaultMessageTimeToLive);
        }

        [Test]
        public void Should_be_able_to_set_DuplicateDetectionHistoryTimeWindow()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var dedupTimespan = TimeSpan.FromMinutes(20);
            extensions.Queues().DuplicateDetectionHistoryTimeWindow(dedupTimespan);

            Assert.AreEqual(dedupTimespan, topology.Settings.QueueSettings.DuplicateDetectionHistoryTimeWindow);
        }

        [Test]
        public void Should_be_able_to_set_EnableBatchedOperations()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().EnableBatchedOperations(true);

            Assert.IsTrue(topology.Settings.QueueSettings.EnableBatchedOperations);
        }

        [Test]
        public void Should_be_able_to_set_EnableDeadLetteringOnMessageExpiration()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().EnableDeadLetteringOnMessageExpiration(true);

            Assert.IsTrue(topology.Settings.QueueSettings.EnableDeadLetteringOnMessageExpiration);
        }

        [Test]
        public void Should_be_able_to_set_EnableExpress()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().EnableExpress(true);

            Assert.IsTrue(topology.Settings.QueueSettings.EnableExpress);
        }

        [Test]
        public void Should_be_able_to_set_EnableExpress_conditionally()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, bool> condition = name => name != "expressqueue";
            extensions.Queues().EnableExpress(condition, true);

            Assert.IsTrue(topology.Settings.QueueSettings.EnableExpress);
            Assert.AreEqual(condition, topology.Settings.QueueSettings.EnableExpressCondition);
        }


        [Test]
        public void Should_be_able_to_set_EnablePartitioning()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().EnablePartitioning(true);

            Assert.IsTrue(topology.Settings.QueueSettings.EnablePartitioning);
        }

        [Test]
        public void Should_be_able_to_set_ForwardDeadLetteredMessagesTo()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().ForwardDeadLetteredMessagesTo("deadletteredmessages");

            Assert.AreEqual("deadletteredmessages", topology.Settings.QueueSettings.ForwardDeadLetteredMessagesTo);
        }

        [Test]
        public void Should_be_able_to_set_ForwardDeadLetteredMessagesTo_conditionally()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, bool> condition = n => n != "deadletteredmessages";
            extensions.Queues().ForwardDeadLetteredMessagesTo(condition, "deadletteredmessages");

            Assert.AreEqual("deadletteredmessages", topology.Settings.QueueSettings.ForwardDeadLetteredMessagesTo);
            Assert.AreEqual(condition, topology.Settings.QueueSettings.ForwardDeadLetteredMessagesToCondition);
        }

        [Test]
        public void Should_be_able_to_set_LockDuration()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var lockDuration = TimeSpan.FromDays(1);
            extensions.Queues().LockDuration(lockDuration);

            Assert.AreEqual(lockDuration, topology.Settings.QueueSettings.LockDuration);
        }

        [Test]
        public void Should_be_able_to_set_MaxDeliveryCount()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            const int selectedMaxDeliveryCount = 6;
            extensions.Queues().MaxDeliveryCount(selectedMaxDeliveryCount);

            Assert.AreEqual(selectedMaxDeliveryCount, topology.Settings.QueueSettings.MaxDeliveryCount);
        }

        [Test]
        public void Should_be_able_to_set_MaxSizeInMegabytes()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            const long maxTopicSizeInMB = 2048;
            extensions.Queues().MaxSizeInMegabytes(SizeInMegabytes.Size2048);

            Assert.AreEqual(maxTopicSizeInMB, (long)topology.Settings.QueueSettings.MaxSizeInMegabytes);
        }

        [Test]
        public void Should_be_able_to_set_RequiresDuplicateDetection()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().RequiresDuplicateDetection(true);

            Assert.True(topology.Settings.QueueSettings.RequiresDuplicateDetection);
        }

        [Test]
        public void Should_be_able_to_set_SupportOrdering()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Queues().SupportOrdering(true);

            Assert.IsTrue(topology.Settings.QueueSettings.SupportOrdering);
        }
    }
}