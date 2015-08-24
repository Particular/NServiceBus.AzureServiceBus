namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusMessagingFactoriesSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusMessagingFactoriesSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        public AzureServiceBusMessagingFactoriesSettings MessagingFactorySettingsFactory(Func<string, MessagingFactorySettings> factory)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessagingFactories.MessagingFactorySettingsFactory, factory);

            return this;
        }

        public AzureServiceBusMessagingFactoriesSettings NumberOfMessagingFactoriesPerNamespace(int number)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace, number);

            return this;
        }


        public AzureServiceBusMessagingFactoriesSettings PrefetchCount(int prefetchCount)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessagingFactories.PrefetchCount, prefetchCount);

            return this;
        }

        public AzureServiceBusMessagingFactoriesSettings RetryPolicy(RetryPolicy retryPolicy)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessagingFactories.RetryPolicy, retryPolicy);

            return this;
        }
    }
}