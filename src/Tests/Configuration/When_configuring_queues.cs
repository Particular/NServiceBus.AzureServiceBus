namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_queues
    {
        public void Should_be_able_to_set_AutoDeleteOnIdle()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var idlePeriod = TimeSpan.FromDays(10);
            var queueSettings = extensions.Queues().AutoDeleteOnIdle(idlePeriod);

            Assert.AreEqual(idlePeriod, queueSettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Queues.AutoDeleteOnIdle));
        }

        public void Should_be_able_to_set_DefaultMessageTimeToLive()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var expiryTimespan = TimeSpan.FromDays(1);
            var queueSettings = extensions.Queues().DefaultMessageTimeToLive(expiryTimespan);

            Assert.AreEqual(expiryTimespan, queueSettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Queues.DefaultMessageTimeToLive));
        }

        public void Should_be_able_to_set_DuplicateDetectionHistoryTimeWindow()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var dedupTimespan = TimeSpan.FromMinutes(20);
            var queueSettings = extensions.Queues().DuplicateDetectionHistoryTimeWindow(dedupTimespan);

            Assert.AreEqual(dedupTimespan, queueSettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Queues.DuplicateDetectionHistoryTimeWindow));
        }

        [Test]
        public void Should_be_able_to_set_EnableBatchedOperations()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            var queueSettings = extensions.Queues().EnableBatchedOperations(true);

            Assert.IsTrue(queueSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableBatchedOperations));
        }

        [Test]
        public void Should_be_able_to_set_EnableDeadLetteringOnMessageExpiration()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            var queueSettings = extensions.Queues().EnableDeadLetteringOnMessageExpiration(true);

            Assert.IsTrue(queueSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableDeadLetteringOnMessageExpiration));
        }

        [Test]
        public void Should_be_able_to_set_EnableExpress()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            var queueSettings = extensions.Queues().EnableExpress(true);

            Assert.IsTrue(queueSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpress));
        }

        [Test]
        public void Should_be_able_to_set_EnableExpress_conditionally()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            Func<string, bool> condition = name => name != "expressqueue";
            var queueSettings = extensions.Queues().EnableExpress(condition, true);

            Assert.IsTrue(queueSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpress));
            Assert.AreEqual(condition, queueSettings.GetSettings().Get<Func<string, bool>>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpressCondition));
        }


        [Test]
        public void Should_be_able_to_set_EnablePartitioning()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            var queueSettings = extensions.Queues().EnablePartitioning(true);

            Assert.IsTrue(queueSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnablePartitioning));
        }

        [Test]
        public void Should_be_able_to_set_ForwardDeadLetteredMessagesTo()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            var queueSettings = extensions.Queues().ForwardDeadLetteredMessagesTo("deadletteredmessages");

            Assert.AreEqual("deadletteredmessages", queueSettings.GetSettings().Get<string>(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesTo));
        }

        [Test]
        public void Should_be_able_to_set_ForwardDeadLetteredMessagesTo_conditionally()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            Func<string, bool> condition = n => n != "deadletteredmessages";
            var queueSettings = extensions.Queues().ForwardDeadLetteredMessagesTo(condition, "deadletteredmessages");

            Assert.AreEqual("deadletteredmessages", queueSettings.GetSettings().Get<string>(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesTo));
            Assert.AreEqual(condition, queueSettings.GetSettings().Get<Func<string, bool>>(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesToCondition));
        }

        [Test]
        public void Should_be_able_to_set_LockDuration()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            var lockDuration = TimeSpan.FromDays(1);
            var queueSettings = extensions.Queues().LockDuration(lockDuration);

            Assert.AreEqual(lockDuration, queueSettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Queues.LockDuration));
        }

        [Test]
        public void Should_be_able_to_set_MaxDeliveryCount()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            const int selectedMaxDeliveryCount = 6;
            var queueSettings = extensions.Queues().MaxDeliveryCount(selectedMaxDeliveryCount);

            Assert.AreEqual(selectedMaxDeliveryCount, queueSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxDeliveryCount));
        }

        [Test]
        public void Should_be_able_to_set_MaxSizeInMegabytes()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            const long maxTopicSizeInMB = 2048;
            var queueSettings = extensions.Queues().MaxSizeInMegabytes(SizeInMegabytes.Size2048);

            Assert.AreEqual(maxTopicSizeInMB, queueSettings.GetSettings().Get<long>(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxSizeInMegabytes));
        }

        [Test]
        public void Should_be_able_to_set_RequiresDuplicateDetection()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var queueSettings = extensions.Queues().RequiresDuplicateDetection(true);

            Assert.True(queueSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresDuplicateDetection));
        }

        [Test]
        public void Should_be_able_to_set_SupportOrdering()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var queueSettings = extensions.Queues().SupportOrdering(true);

            Assert.IsTrue(queueSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.SupportOrdering));
        }
    }
}