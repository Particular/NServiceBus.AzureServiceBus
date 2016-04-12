namespace NServiceBus
{
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
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
        /// Rules to apply for entity path/name validation.
        /// <remarks> Default is <see cref="EntityNameValidationRules"/>. For backwards compatibility, use <see cref="EntityNameValidationV6Rules"/>.</remarks>
        /// </summary>
        public AzureServiceBusValidationSettings UseStrategy<T>() where T : IValidationStrategy
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy, typeof(T));

            return this;
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
    }
}