namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusTopicSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusTopicSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        public AzureServiceBusTopicSettings DescriptionFactory(Func<string, ReadOnlySettings, TopicDescription> factory)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.DescriptionFactory, factory);

            return this;
        }

        public AzureServiceBusTopicSettings SupportOrdering(bool supported)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.SupportOrdering, supported);

            return this;
        }
    }
}