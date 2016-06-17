namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_sanitization
    {
        SettingsHolder settingsHolder;
        TransportExtensions<AzureServiceBusTransport> extensions;

        [SetUp]
        public void SetUp()
        {
            settingsHolder = new SettingsHolder();
            extensions = new TransportExtensions<AzureServiceBusTransport>(settingsHolder);
        }


        [Test]
        public void Should_be_able_to_set_queue_path_maximum_length()
        {
            var validationSettings = extensions.Sanitization().UseQueuePathMaximumLength(10);

            Assert.AreEqual(10, validationSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathMaximumLength));
        }

        [Test]
        public void Should_be_able_to_set_topic_path_maximum_length()
        {
            var validationSettings = extensions.Sanitization().UseTopicPathMaximumLength(20);

            Assert.AreEqual(20, validationSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathMaximumLength));
        }

        [Test]
        public void Should_be_able_to_set_subscription_path_maximum_length()
        {
            var validationSettings = extensions.Sanitization().UseSubscriptionPathMaximumLength(30);

            Assert.AreEqual(30, validationSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameMaximumLength));
        }

        [Test]
        public void Should_be_able_to_set_queue_path_validator()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, ValidationResult> queuePathValidator = path => new ValidationResult();
            var sanitization = extensions.Sanitization().QueuePathValidation(queuePathValidator);

            Assert.That(queuePathValidator, Is.EqualTo(sanitization.GetSettings().Get<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathValidator)));
        }

        [Test]
        public void Should_be_able_to_set_topic_path_validator()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, ValidationResult> topicPathValidator = path => new ValidationResult();
            var sanitization = extensions.Sanitization().TopicPathValidation(topicPathValidator);

            Assert.That(topicPathValidator, Is.EqualTo(sanitization.GetSettings().Get<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathValidator)));
        }

        [Test]
        public void Should_be_able_to_set_subscription_name_validator()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, ValidationResult> subscriptionNameValidator = name => new ValidationResult();
            var sanitization = extensions.Sanitization().SubscriptionNameValidation(subscriptionNameValidator);

            Assert.That(subscriptionNameValidator, Is.EqualTo(sanitization.GetSettings().Get<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameValidator)));
        }

        [Test]
        public void Should_be_able_to_set_rule_name_validator()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, ValidationResult> ruleNameValidator = name => new ValidationResult();
            var sanitization = extensions.Sanitization().RuleNameValidation(ruleNameValidator);

            Assert.That(ruleNameValidator, Is.EqualTo(sanitization.GetSettings().Get<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameValidator)));
        }

        [Test]
        public void Should_be_able_to_set_queue_path_sanitizer()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, string> queuePathSanitizer = path => path;
            var sanitization = extensions.Sanitization().QueuePathSanitization(queuePathSanitizer);

            Assert.That(queuePathSanitizer, Is.EqualTo(sanitization.GetSettings().Get<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathSanitizer)));
        }

        [Test]
        public void Should_be_able_to_set_topic_path_sanitizer()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, string> topicPathSanitizer = path => path;
            var sanitization = extensions.Sanitization().TopicPathSanitization(topicPathSanitizer);

            Assert.That(topicPathSanitizer, Is.EqualTo(sanitization.GetSettings().Get<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathSanitizer)));
        }

        [Test]
        public void Should_be_able_to_set_subscription_name_sanitizer()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, string> subscriptionNameSanitizer = path => path;
            extensions.Sanitization().SubscriptionNameSanitization(subscriptionNameSanitizer);

            Assert.That(subscriptionNameSanitizer, Is.EqualTo(settings.Get<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameSanitizer)));
        }

        [Test]
        public void Should_be_able_to_set_rule_name_sanitizer()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, string> ruleNameSanitizer = path => path;
            extensions.Sanitization().RuleNameSanitization(ruleNameSanitizer);

            Assert.That(ruleNameSanitizer, Is.EqualTo(settings.Get<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameSanitizer)));
        }
    }
}