namespace NServiceBus
{
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
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionPathMaximumLength, subscriptionPathMaximumLength);

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
        /// Rules to apply for entity path/name sanitization.
        /// <remarks> Default is <see cref="ThrowOnFailingSanitization"/>. For backward compatibility with <see cref="EndpointOrientedTopology"/> use <see cref="EndpointOrientedTopologySanitization"/>.</remarks>
        /// </summary>
        public AzureServiceBusSanitizationSettings UseStrategy<T>() where T : ISanitizationStrategy
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy, typeof(T));

            return this;
        }
    }
}