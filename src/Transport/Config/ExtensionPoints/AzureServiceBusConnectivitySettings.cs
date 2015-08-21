namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusConnectivitySettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusConnectivitySettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        public AzureServiceBusConnectivitySettings MessagingFactorySettings(Func<string, MessagingFactorySettings> factory)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessagingFactorySettingsFactory, factory);

            return this;
        }

        public AzureServiceBusConnectivitySettings NumberOfMessagingFactoriesPerNamespace(int number)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.NumberOfMessagingFactoriesPerNamespace, number);

            return this;
        }

        public AzureServiceBusConnectivitySettings NumberOfClientsPerEntity(int number)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity, number);

            return this;
        }
    }
}