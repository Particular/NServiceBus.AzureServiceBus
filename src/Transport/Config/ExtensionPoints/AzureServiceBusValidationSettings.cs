namespace NServiceBus
{
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusValidationSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusValidationSettings(SettingsHolder settings) : base(settings)
        {
            _settings = settings;
        }

        public AzureServiceBusValidationSettings UseStrategy<T>() where T : IValidationStrategy
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy, typeof(T));

            return this;
        }

        public AzureServiceBusValidationSettings UseQueuePathMaximumLength(int queuePathMaximumLength)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Validation.QueuePathMaximumLength, queuePathMaximumLength);

            return this;
        }

        public AzureServiceBusValidationSettings UseTopicPathMaximumLength(int topicPathMaximumLength)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Validation.TopicPathMaximumLength, topicPathMaximumLength);

            return this;
        }

        public AzureServiceBusValidationSettings UseSubscriptionPathMaximumLength(int subscriptionPathMaximumLength)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Validation.SubscriptionPathMaximumLength, subscriptionPathMaximumLength);

            return this;
        }
    }
}