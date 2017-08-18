namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using Transport.AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_sanitization_strategy
    {

        [Test]
        public void Should_be_able_to_set_the_sanitization_strategy()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var strategy = new MySanitizationStrategy();
            var strategySettings = extensions.Sanitization().UseStrategy(strategy);

            Assert.AreSame(strategy, strategySettings.GetSettings().Get<ISanitizationStrategy>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy));
        }

        [Test]
        public void Should_be_able_to_set_hash()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, string> hash = pathOrName => pathOrName;
            extensions.Sanitization().UseStrategy(new MySanitizationStrategy()).Hash(hash);

            Assert.That(hash, Is.EqualTo(settings.Get<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Hash)));
        }

        [Test]
        public void Should_be_able_to_set_queue_path_validator()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, ValidationResult> queuePathValidator = path => new ValidationResult();
            var sanitization = extensions.Sanitization().UseStrategy(new MySanitizationStrategy()).QueuePathValidation(queuePathValidator);

            Assert.That(queuePathValidator, Is.EqualTo(sanitization.GetSettings().Get<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathValidator)));
        }

        [Test]
        public void Should_be_able_to_set_topic_path_validator()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, ValidationResult> topicPathValidator = path => new ValidationResult();
            var sanitization = extensions.Sanitization().UseStrategy(new MySanitizationStrategy()).TopicPathValidation(topicPathValidator);

            Assert.That(topicPathValidator, Is.EqualTo(sanitization.GetSettings().Get<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathValidator)));
        }

        [Test]
        public void Should_be_able_to_set_subscription_name_validator()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, ValidationResult> subscriptionNameValidator = name => new ValidationResult();
            var sanitization = extensions.Sanitization().UseStrategy(new MySanitizationStrategy()).SubscriptionNameValidation(subscriptionNameValidator);

            Assert.That(subscriptionNameValidator, Is.EqualTo(sanitization.GetSettings().Get<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameValidator)));
        }

        [Test]
        public void Should_be_able_to_set_rule_name_validator()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, ValidationResult> ruleNameValidator = name => new ValidationResult();
            var sanitization = extensions.Sanitization().UseStrategy(new MySanitizationStrategy()).RuleNameValidation(ruleNameValidator);

            Assert.That(ruleNameValidator, Is.EqualTo(sanitization.GetSettings().Get<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameValidator)));
        }

        [Test]
        public void Should_be_able_to_set_queue_path_sanitizer()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, string> queuePathSanitizer = path => path;
            var sanitization = extensions.Sanitization().UseStrategy(new MySanitizationStrategy()).QueuePathSanitization(queuePathSanitizer);

            Assert.That(queuePathSanitizer, Is.EqualTo(sanitization.GetSettings().Get<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathSanitizer)));
        }

        [Test]
        public void Should_be_able_to_set_topic_path_sanitizer()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, string> topicPathSanitizer = path => path;
            var sanitization = extensions.Sanitization().UseStrategy(new MySanitizationStrategy()).TopicPathSanitization(topicPathSanitizer);

            Assert.That(topicPathSanitizer, Is.EqualTo(sanitization.GetSettings().Get<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathSanitizer)));
        }

        [Test]
        public void Should_be_able_to_set_subscription_name_sanitizer()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, string> subscriptionNameSanitizer = path => path;
            extensions.Sanitization().UseStrategy(new MySanitizationStrategy()).SubscriptionNameSanitization(subscriptionNameSanitizer);

            Assert.That(subscriptionNameSanitizer, Is.EqualTo(settings.Get<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameSanitizer)));
        }

        [Test]
        public void Should_be_able_to_set_rule_name_sanitizer()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, string> ruleNameSanitizer = path => path;
            extensions.Sanitization().UseStrategy(new MySanitizationStrategy()).RuleNameSanitization(ruleNameSanitizer);

            Assert.That(ruleNameSanitizer, Is.EqualTo(settings.Get<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameSanitizer)));
        }

        class MySanitizationStrategy : ISanitizationStrategy
        {
            public void Initialize(ReadOnlySettings settings)
            {
                throw new NotImplementedException();
            }

            public string Sanitize(string entityPathOrName, EntityType entityType)
            {
                throw new NotImplementedException();//not relevant for test
            }
        }
    }
}