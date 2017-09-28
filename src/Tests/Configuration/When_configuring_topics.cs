namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using Settings;
    using NUnit.Framework;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_topics
    {
        [Test]
        public void Should_be_able_to_set_AutoDeleteOnIdle()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topics().AutoDeleteOnIdle(TimeSpan.MinValue);

            Assert.AreEqual(TimeSpan.MinValue, settings.Get<TopologySettings>().TopicSettings.AutoDeleteOnIdle);
        }

        [Test]
        public void Should_be_able_to_set_DefaultMessageTimeToLive()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var timeToLive = TimeSpan.FromHours(5);
            extensions.Topics().DefaultMessageTimeToLive(timeToLive);

            Assert.AreEqual(timeToLive, settings.Get<TopologySettings>().TopicSettings.DefaultMessageTimeToLive);
        }

        [Test]
        public void Should_be_able_to_set_DuplicateDetectionHistoryTimeWindow()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var dedupTimeWindow = TimeSpan.FromMilliseconds(30000);
            extensions.Topics().DuplicateDetectionHistoryTimeWindow(dedupTimeWindow);

            Assert.AreEqual(dedupTimeWindow, settings.Get<TopologySettings>().TopicSettings.DuplicateDetectionHistoryTimeWindow);
        }

        [Test]
        public void Should_be_able_to_set_EnableBatchedOperations()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topics().EnableBatchedOperations(true);

            Assert.IsTrue(settings.Get<TopologySettings>().TopicSettings.EnableBatchedOperations);
        }

        [Test]
        public void Should_be_able_to_set_EnableExpress()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topics().EnableExpress(true);

            Assert.IsTrue(settings.Get<TopologySettings>().TopicSettings.EnableExpress);
        }

        [Test]
        public void Should_be_able_to_set_EnableExpress_conditionally()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, bool> condition = name => name != "expresstopic";
            extensions.Topics().EnableExpress(condition, true);

            Assert.IsTrue(settings.Get<TopologySettings>().TopicSettings.EnableExpress);
            Assert.AreEqual(condition, settings.Get<TopologySettings>().TopicSettings.EnableExpressCondition);
        }

        [Test]
        public void Should_be_able_to_set_EnableFilteringMessagesBeforePublishing()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topics().EnableFilteringMessagesBeforePublishing(true);

            Assert.IsTrue(settings.Get<TopologySettings>().TopicSettings.EnableFilteringMessagesBeforePublishing);
        }

        [Test]
        public void Should_be_able_to_set_EnablePartitioning()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

           extensions.Topics().EnablePartitioning(true);

            Assert.IsTrue(settings.Get<TopologySettings>().TopicSettings.EnablePartitioning);
        }

        [Test]
        public void Should_be_able_to_set_MaxSizeInMegabytes()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topics().MaxSizeInMegabytes(SizeInMegabytes.Size2048);

            Assert.AreEqual(SizeInMegabytes.Size2048, settings.Get<TopologySettings>().TopicSettings.MaxSizeInMegabytes);
        }

        [Test]
        public void Should_be_able_to_set_RequiresDuplicateDetection()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topics().RequiresDuplicateDetection(true);

            Assert.True(settings.Get<TopologySettings>().TopicSettings.RequiresDuplicateDetection);
        }

        [Test]
        public void Should_be_able_to_set_SupportOrdering()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topics().SupportOrdering(true);

            Assert.IsTrue(settings.Get<TopologySettings>().TopicSettings.SupportOrdering);
        }

        [Test]
        public void Should_be_able_to_set_topic_description_factory_method()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Action<TopicDescription> registeredFactory = td => { };

            extensions.Topics().DescriptionCustomizer(registeredFactory);

            Assert.AreEqual(registeredFactory, settings.Get<TopologySettings>().TopicSettings.DescriptionCustomizer);
        }

    }
}