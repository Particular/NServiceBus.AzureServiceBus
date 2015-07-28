namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusQueueSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusQueueSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        public AzureServiceBusQueueSettings DescriptionFactory(Func<string, ReadOnlySettings, QueueDescription> factory)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.DescriptionFactory, factory);

            return this;
        }

        public AzureServiceBusQueueSettings SupportOrdering(bool supported)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.SupportOrdering, supported);

            return this;
        }


        
    }
}