namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusResourceSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusResourceSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        public AzureServiceBusResourceSettings QueueDescriptions(Func<string, QueueDescription> factory)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.QueueDescriptionsFactory, factory);

            return this;
        }

        public AzureServiceBusResourceSettings TopicDescriptions(Func<string, TopicDescription> factory)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.TopicDescriptionsFactory, factory);

            return this;
        }

        public AzureServiceBusResourceSettings SubscriptionDescriptions(Func<string, SubscriptionDescription> factory)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.SubscriptionDescriptionsFactory, factory);

            return this;
        }
    }
}
