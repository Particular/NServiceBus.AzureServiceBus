namespace NServiceBus
{
    using System;
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusSanitizationExtensionPoint<T> : ExposeSettings where T : ISanitizationStrategy
    {
        SettingsHolder settings;

        internal AzureServiceBusSanitizationExtensionPoint(SettingsHolder settings) : base(settings)
        {
            this.settings = settings;
        }

        public AzureServiceBusSanitizationExtensionPoint<T> QueuePathValidation(Func<string, ValidationResult> queuePathValidator)
        {
            Guard.AgainstNull(nameof(queuePathValidator), queuePathValidator);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathValidator, queuePathValidator);
            return this;
        }

        public AzureServiceBusSanitizationExtensionPoint<T> TopicPathValidation(Func<string, ValidationResult> topicPathValidator)
        {
            Guard.AgainstNull(nameof(topicPathValidator), topicPathValidator);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathValidator, topicPathValidator);
            return this;
        }

        public AzureServiceBusSanitizationExtensionPoint<T> SubscriptionNameValidation(Func<string, ValidationResult> subscriptionNameValidator)
        {
            Guard.AgainstNull(nameof(subscriptionNameValidator), subscriptionNameValidator);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameValidator, subscriptionNameValidator);
            return this;
        }

        public AzureServiceBusSanitizationExtensionPoint<T> RuleNameValidation(Func<string, ValidationResult> ruleNameValidator)
        {
            Guard.AgainstNull(nameof(ruleNameValidator), ruleNameValidator);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameValidator, ruleNameValidator);
            return this;
        }

        public AzureServiceBusSanitizationExtensionPoint<T> QueuePathSanitization(Func<string, string> queuePathSanitizer)
        {
            Guard.AgainstNull(nameof(queuePathSanitizer), queuePathSanitizer);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathSanitizer, queuePathSanitizer);
            return this;

        }

        public AzureServiceBusSanitizationExtensionPoint<T> TopicPathSanitization(Func<string, string> topicPathSanitizer)
        {
            Guard.AgainstNull(nameof(topicPathSanitizer), topicPathSanitizer);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathSanitizer, topicPathSanitizer);
            return this;
        }

        public AzureServiceBusSanitizationExtensionPoint<T> SubscriptionNameSanitization(Func<string, string> subscriptionNameSanitizer)
        {
            Guard.AgainstNull(nameof(subscriptionNameSanitizer), subscriptionNameSanitizer);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameSanitizer, subscriptionNameSanitizer);
            return this;
        }

        public AzureServiceBusSanitizationExtensionPoint<T> RuleNameSanitization(Func<string, string> ruleNameSanitizer)
        {
            Guard.AgainstNull(nameof(ruleNameSanitizer), ruleNameSanitizer);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameSanitizer, ruleNameSanitizer);
            return this;
        }

        public AzureServiceBusSanitizationExtensionPoint<T> Hash(Func<string, string> hash)
        {
            Guard.AgainstNull(nameof(hash), hash);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Hash, hash);
            return this;
        }
    }
}