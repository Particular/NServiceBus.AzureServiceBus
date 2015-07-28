namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusSubscriptionSettings : ExposeSettings
    {
         SettingsHolder _settings;

         public AzureServiceBusSubscriptionSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

         public AzureServiceBusSubscriptionSettings DescriptionFactory(Func<string, string, ReadOnlySettings, SubscriptionDescription> factory)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.DescriptionFactory, factory);

            return this;
        }

         public AzureServiceBusSubscriptionSettings SupportOrdering(bool supported)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.SupportOrdering, supported);

            return this;
        }
    }
}