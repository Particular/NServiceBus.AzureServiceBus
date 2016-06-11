namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_sanitization
    {
        [Test]
        public void Should_be_able_to_set_queue_path_maximum_length()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            var validationSettings = extensions.Sanitization().UseQueuePathMaximumLength(10);

            Assert.AreEqual(10, validationSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathMaximumLength));
        }

        [Test]
        public void Should_be_able_to_set_topic_path_maximum_length()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            var sanitizationSettings = extensions.Sanitization().UseTopicPathMaximumLength(20);

            Assert.AreEqual(20, sanitizationSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathMaximumLength));
        }

        [Test]
        public void Should_be_able_to_set_subscription_path_maximum_length()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            var sanitizationSettings = extensions.Sanitization().UseSubscriptionPathMaximumLength(30);

            Assert.AreEqual(30, sanitizationSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionPathMaximumLength));
        }

        [Test]
        public void Should_be_able_to_set_the_sanitization_strategy()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var mySanitizationStrategy = new MySanitizationStrategy();
            var topicSettings = extensions.Sanitization().UseStrategy(mySanitizationStrategy);

            var found = topicSettings.GetSettings().Get<HashSet<SanitizationStrategy>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy);
            Assert.That(found, Is.InstanceOf<HashSet<SanitizationStrategy>>());
            Assert.That(found.First(), Is.EqualTo(mySanitizationStrategy));
        }

        class MySanitizationStrategy : SanitizationStrategy
        {
            public override string Sanitize(string entityPathOrName)
            {
                throw new NotImplementedException();//not relevant for test
            }

            public override EntityType CanSanitize { get; } = EntityType.Queue;
        }
    }
}