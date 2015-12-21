namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_topics
    {
        [Test]
        public void Should_be_able_to_set_AutoDeleteOnIdle()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var topicSettings = extensions.UseDefaultTopology().Resources().Topics().AutoDeleteOnIdle(TimeSpan.MinValue);

            Assert.AreEqual(TimeSpan.MinValue, topicSettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Topics.AutoDeleteOnIdle));
        }

        [Test]
        public void Should_be_able_to_set_DefaultMessageTimeToLive()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var timeToLive = TimeSpan.FromHours(5);
            var topicSettings = extensions.UseDefaultTopology().Resources().Topics().DefaultMessageTimeToLive(timeToLive);

            Assert.AreEqual(timeToLive, topicSettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Topics.DefaultMessageTimeToLive));
        }

        [Test]
        public void Should_be_able_to_set_DuplicateDetectionHistoryTimeWindow()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var dedupTimeWindow = TimeSpan.FromMilliseconds(30000);
            var topicSettings = extensions.UseDefaultTopology().Resources().Topics().DuplicateDetectionHistoryTimeWindow(dedupTimeWindow);

            Assert.AreEqual(dedupTimeWindow, topicSettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Topics.DuplicateDetectionHistoryTimeWindow));
        }

        [Test]
        public void Should_be_able_to_set_EnableBatchedOperations()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var topicSettings = extensions.UseDefaultTopology().Resources().Topics().EnableBatchedOperations(true);

            Assert.IsTrue(topicSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableBatchedOperations));
        }

        [Test]
        public void Should_be_able_to_set_EnableExpress()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var topicSettings = extensions.UseDefaultTopology().Resources().Topics().EnableExpress(true);

            Assert.IsTrue(topicSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableExpress));
        }

        [Test]
        public void Should_be_able_to_set_EnableExpress_conditionally()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, bool> condition = name => name != "expresstopic";
            var topicSettings = extensions.UseDefaultTopology().Resources().Topics().EnableExpress(condition, true);

            Assert.IsTrue(topicSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableExpress));
            Assert.AreEqual(condition, topicSettings.GetSettings().Get<Func<string, bool>>(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableExpressCondition));
        }

        [Test]
        public void Should_be_able_to_set_EnableFilteringMessagesBeforePublishing()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var topicSettings = extensions.UseDefaultTopology().Resources().Topics().EnableFilteringMessagesBeforePublishing(true);

            Assert.IsTrue(topicSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableFilteringMessagesBeforePublishing));
        }

        [Test]
        public void Should_be_able_to_set_EnablePartitioning()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var topicSettings = extensions.UseDefaultTopology().Resources().Topics().EnablePartitioning(true);

            Assert.IsTrue(topicSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Topics.EnablePartitioning));
        }

        [Test]
        public void Should_be_able_to_set_MaxSizeInMegabytes()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            const long maxTopicSizeInMB = 2048;
            var topicSettings = extensions.UseDefaultTopology().Resources().Topics().MaxSizeInMegabytes(maxTopicSizeInMB);

            Assert.AreEqual(maxTopicSizeInMB, topicSettings.GetSettings().Get<long>(WellKnownConfigurationKeys.Topology.Resources.Topics.MaxSizeInMegabytes));
        }

        [Test]
        public void Should_be_able_to_set_RequiresDuplicateDetection()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var topicSettings = extensions.UseDefaultTopology().Resources().Topics().RequiresDuplicateDetection(true);

            Assert.True(topicSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Topics.RequiresDuplicateDetection));
        }

        [Test]
        public void Should_be_able_to_set_SupportOrdering()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var topicSettings = extensions.UseDefaultTopology().Resources().Topics().SupportOrdering(true);

            Assert.IsTrue(topicSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Topics.SupportOrdering));
        }
    }
}