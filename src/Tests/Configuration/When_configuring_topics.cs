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
        SettingsHolder settings;
        TransportExtensions<AzureServiceBusTransport> extensions;

        [SetUp]
        public void SetUp()
        {
            settings = new SettingsHolder();
            settings.Set<TopologySettings>(new TopologySettings());
            extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
        }

        [Test]
        public void Should_be_able_to_set_AutoDeleteOnIdle()
        {
            extensions.Topics().AutoDeleteOnIdle(TimeSpan.MinValue);

            Assert.AreEqual(TimeSpan.MinValue, settings.Get<TopologySettings>().TopicSettings.AutoDeleteOnIdle);
        }

        [Test]
        public void Should_be_able_to_set_DefaultMessageTimeToLive()
        {
            var timeToLive = TimeSpan.FromHours(5);
            extensions.Topics().DefaultMessageTimeToLive(timeToLive);

            Assert.AreEqual(timeToLive, settings.Get<TopologySettings>().TopicSettings.DefaultMessageTimeToLive);
        }

        [Test]
        public void Should_be_able_to_set_DuplicateDetectionHistoryTimeWindow()
        {
            var dedupTimeWindow = TimeSpan.FromMilliseconds(30000);
            extensions.Topics().DuplicateDetectionHistoryTimeWindow(dedupTimeWindow);

            Assert.AreEqual(dedupTimeWindow, settings.Get<TopologySettings>().TopicSettings.DuplicateDetectionHistoryTimeWindow);
        }

        [Test]
        public void Should_be_able_to_set_EnableBatchedOperations()
        {
            extensions.Topics().EnableBatchedOperations(true);

            Assert.IsTrue(settings.Get<TopologySettings>().TopicSettings.EnableBatchedOperations);
        }

        [Test]
        public void Should_be_able_to_set_EnableExpress()
        {
            extensions.Topics().EnableExpress(true);

            Assert.IsTrue(settings.Get<TopologySettings>().TopicSettings.EnableExpress);
        }

        [Test]
        public void Should_be_able_to_set_EnableExpress_conditionally()
        {
            Func<string, bool> condition = name => name != "expresstopic";
            extensions.Topics().EnableExpress(condition, true);

            Assert.IsTrue(settings.Get<TopologySettings>().TopicSettings.EnableExpress);
            Assert.AreEqual(condition, settings.Get<TopologySettings>().TopicSettings.EnableExpressCondition);
        }

        [Test]
        public void Should_be_able_to_set_EnableFilteringMessagesBeforePublishing()
        {
            extensions.Topics().EnableFilteringMessagesBeforePublishing(true);

            Assert.IsTrue(settings.Get<TopologySettings>().TopicSettings.EnableFilteringMessagesBeforePublishing);
        }

        [Test]
        public void Should_be_able_to_set_EnablePartitioning()
        {
           extensions.Topics().EnablePartitioning(true);

            Assert.IsTrue(settings.Get<TopologySettings>().TopicSettings.EnablePartitioning);
        }

        [Test]
        public void Should_be_able_to_set_MaxSizeInMegabytes()
        {
            extensions.Topics().MaxSizeInMegabytes(SizeInMegabytes.Size2048);

            Assert.AreEqual(SizeInMegabytes.Size2048, settings.Get<TopologySettings>().TopicSettings.MaxSizeInMegabytes);
        }

        [Test]
        public void Should_be_able_to_set_RequiresDuplicateDetection()
        {
            extensions.Topics().RequiresDuplicateDetection(true);

            Assert.True(settings.Get<TopologySettings>().TopicSettings.RequiresDuplicateDetection);
        }

        [Test]
        public void Should_be_able_to_set_SupportOrdering()
        {
            extensions.Topics().SupportOrdering(true);

            Assert.IsTrue(settings.Get<TopologySettings>().TopicSettings.SupportOrdering);
        }

        [Test]
        public void Should_be_able_to_set_topic_description_factory_method()
        {
            Action<TopicDescription> registeredFactory = td => { };

            extensions.Topics().DescriptionCustomizer(registeredFactory);

            Assert.AreEqual(registeredFactory, settings.Get<TopologySettings>().TopicSettings.DescriptionCustomizer);
        }

    }
}