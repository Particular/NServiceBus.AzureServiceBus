namespace NServiceBus
{
    using AzureServiceBus;
    using Configuration.AdvanceExtensibility;
    using Settings;

    public class AzureServiceBusValidationSettings : ExposeSettings
    {
        SettingsHolder settings;

        public AzureServiceBusValidationSettings(SettingsHolder settings) : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// <remarks> Default is 260 characters.</remarks>
        /// </summary>
        public AzureServiceBusValidationSettings UseQueuePathMaximumLength(int queuePathMaximumLength)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Validation.QueuePathMaximumLength, queuePathMaximumLength);

            return this;
        }

        /// <summary>
        /// <remarks> Default is 260 characters.</remarks>
        /// </summary>
        public AzureServiceBusValidationSettings UseTopicPathMaximumLength(int topicPathMaximumLength)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Validation.TopicPathMaximumLength, topicPathMaximumLength);

            return this;
        }

        /// <summary>
        /// <remarks> Default is 50 characters.</remarks>
        /// </summary>
        public AzureServiceBusValidationSettings UseSubscriptionPathMaximumLength(int subscriptionPathMaximumLength)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Validation.SubscriptionPathMaximumLength, subscriptionPathMaximumLength);

            return this;
        }

        /// <summary>
        /// <remarks> Default is 50 characters.</remarks>
        /// </summary>
        public AzureServiceBusValidationSettings UseRulePathMaximumLength(int rulePathMaximumLength)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Validation.RuleNameMaximumLength, rulePathMaximumLength);

            return this;
        }
    }
}