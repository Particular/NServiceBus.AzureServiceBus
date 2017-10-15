namespace NServiceBus
{
    using System;
    using Configuration.AdvancedExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    /// <summary>
    /// Sanitization configuration settings.
    /// </summary>
    public class AzureServiceBusSanitizationExtensionPoint<T> : ExposeSettings where T : ISanitizationStrategy
    {
        internal AzureServiceBusSanitizationExtensionPoint(SettingsHolder settings) : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Queue path validator to be used.
        /// </summary>
        public AzureServiceBusSanitizationExtensionPoint<T> QueuePathValidation(Func<string, ValidationResult> queuePathValidator)
        {
            Guard.AgainstNull(nameof(queuePathValidator), queuePathValidator);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathValidator, queuePathValidator);
            return this;
        }

        /// <summary>
        /// Topic path validator to be used.
        /// </summary>
        public AzureServiceBusSanitizationExtensionPoint<T> TopicPathValidation(Func<string, ValidationResult> topicPathValidator)
        {
            Guard.AgainstNull(nameof(topicPathValidator), topicPathValidator);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathValidator, topicPathValidator);
            return this;
        }

        /// <summary>
        /// Subscription name validator to be used.
        /// </summary>
        public AzureServiceBusSanitizationExtensionPoint<T> SubscriptionNameValidation(Func<string, ValidationResult> subscriptionNameValidator)
        {
            Guard.AgainstNull(nameof(subscriptionNameValidator), subscriptionNameValidator);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameValidator, subscriptionNameValidator);
            return this;
        }

        /// <summary>
        /// Rule name validator to be used.
        /// </summary>

        public AzureServiceBusSanitizationExtensionPoint<T> RuleNameValidation(Func<string, ValidationResult> ruleNameValidator)
        {
            Guard.AgainstNull(nameof(ruleNameValidator), ruleNameValidator);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameValidator, ruleNameValidator);
            return this;
        }

        /// <summary>
        /// Queue path sanitizer to be used.
        /// </summary>
        public AzureServiceBusSanitizationExtensionPoint<T> QueuePathSanitization(Func<string, string> queuePathSanitizer)
        {
            Guard.AgainstNull(nameof(queuePathSanitizer), queuePathSanitizer);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathSanitizer, queuePathSanitizer);
            return this;
        }

        /// <summary>
        /// Topic path sanitizer to be used.
        /// </summary>
        public AzureServiceBusSanitizationExtensionPoint<T> TopicPathSanitization(Func<string, string> topicPathSanitizer)
        {
            Guard.AgainstNull(nameof(topicPathSanitizer), topicPathSanitizer);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathSanitizer, topicPathSanitizer);
            return this;
        }

        /// <summary>
        /// Subscription name sanitizer to be used.
        /// </summary>
        public AzureServiceBusSanitizationExtensionPoint<T> SubscriptionNameSanitization(Func<string, string> subscriptionNameSanitizer)
        {
            Guard.AgainstNull(nameof(subscriptionNameSanitizer), subscriptionNameSanitizer);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameSanitizer, subscriptionNameSanitizer);
            return this;
        }

        /// <summary>
        /// Rule name sanitizer to be used.
        /// </summary>
        public AzureServiceBusSanitizationExtensionPoint<T> RuleNameSanitization(Func<string, string> ruleNameSanitizer)
        {
            Guard.AgainstNull(nameof(ruleNameSanitizer), ruleNameSanitizer);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameSanitizer, ruleNameSanitizer);
            return this;
        }

        /// <summary>
        /// Hasing algorithm to use for shortening.
        /// </summary>
        public AzureServiceBusSanitizationExtensionPoint<T> Hash(Func<string, string> hash)
        {
            Guard.AgainstNull(nameof(hash), hash);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Hash, hash);
            return this;
        }

        SettingsHolder settings;
    }
}