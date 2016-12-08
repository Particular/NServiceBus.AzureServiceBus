namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusSanitizationSettings : ExposeSettings
    {
        SettingsHolder settings;

        internal AzureServiceBusSanitizationSettings(SettingsHolder settings) : base(settings)
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
    }
}