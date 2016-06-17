namespace NServiceBus
{
    using System;
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using Configuration.AdvanceExtensibility;
    using Settings;

    public class AzureServiceBusSanitizationSettings : ExposeSettings
    {
        SettingsHolder settings;

        public AzureServiceBusSanitizationSettings(SettingsHolder settings) : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// <remarks> Default is 260 characters.</remarks>
        /// </summary>
        public AzureServiceBusSanitizationSettings UseQueuePathMaximumLength(int queuePathMaximumLength)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathMaximumLength, queuePathMaximumLength);

            return this;
        }

        /// <summary>
        /// <remarks> Default is 260 characters.</remarks>
        /// </summary>
        public AzureServiceBusSanitizationSettings UseTopicPathMaximumLength(int topicPathMaximumLength)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathMaximumLength, topicPathMaximumLength);

            return this;
        }

        /// <summary>
        /// <remarks> Default is 50 characters.</remarks>
        /// </summary>
        public AzureServiceBusSanitizationSettings UseSubscriptionPathMaximumLength(int subscriptionPathMaximumLength)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameMaximumLength, subscriptionPathMaximumLength);

            return this;
        }

        /// <summary>
        /// <remarks> Default is 50 characters.</remarks>
        /// </summary>
        public AzureServiceBusSanitizationSettings UseRulePathMaximumLength(int rulePathMaximumLength)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameMaximumLength, rulePathMaximumLength);

            return this;
        }


        /// <summary>
        /// Strategy to use for sanitization of entity paths/names.
        /// <remarks> Default is <see cref="ThrowOnFailedValidation"/>. For backward compatibility with <see cref="EndpointOrientedTopology"/> use <see cref="ValidateAndHashIfNeeded"/>.</remarks>
        /// </summary>
        public AzureServiceBusSanitizationExtensionPoint<T> UseStrategy<T>() where T : ISanitizationStrategy
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy, typeof(T));

            return new AzureServiceBusSanitizationExtensionPoint<T>(settings);
        }

        public AzureServiceBusSanitizationSettings QueuePathValidation(Func<string, ValidationResult> queuePathValidator)
        {
            Guard.AgainstNull(nameof(queuePathValidator), queuePathValidator);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathValidator, queuePathValidator);
            return this;
        }

        public AzureServiceBusSanitizationSettings TopicPathValidation(Func<string, ValidationResult> topicPathValidator)
        {
            Guard.AgainstNull(nameof(topicPathValidator), topicPathValidator);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathValidator, topicPathValidator);
            return this;
        }

        public AzureServiceBusSanitizationSettings SubscriptionNameValidation(Func<string, ValidationResult> subscriptionNameValidator)
        {
            Guard.AgainstNull(nameof(subscriptionNameValidator), subscriptionNameValidator);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameValidator, subscriptionNameValidator);
            return this;
        }

        public AzureServiceBusSanitizationSettings RuleNameValidation(Func<string, ValidationResult> ruleNameValidator)
        {
            Guard.AgainstNull(nameof(ruleNameValidator), ruleNameValidator);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameValidator, ruleNameValidator);
            return this;
        }

        public AzureServiceBusSanitizationSettings QueuePathSanitization(Func<string, string> queuePathSanitizer)
        {
            Guard.AgainstNull(nameof(queuePathSanitizer), queuePathSanitizer);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathSanitizer, queuePathSanitizer);
            return this;

        }

        public AzureServiceBusSanitizationSettings TopicPathSanitization(Func<string, string> topicPathSanitizer)
        {
            Guard.AgainstNull(nameof(topicPathSanitizer), topicPathSanitizer);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathSanitizer, topicPathSanitizer);
            return this;
        }

        public AzureServiceBusSanitizationSettings SubscriptionNameSanitization(Func<string, string> subscriptionNameSanitizer)
        {
            Guard.AgainstNull(nameof(subscriptionNameSanitizer), subscriptionNameSanitizer);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameSanitizer, subscriptionNameSanitizer);
            return this;
        }

        public AzureServiceBusSanitizationSettings RuleNameSanitization(Func<string, string> ruleNameSanitizer)
        {
            Guard.AgainstNull(nameof(ruleNameSanitizer), ruleNameSanitizer);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameSanitizer, ruleNameSanitizer);
            return this;
        }
    }
}