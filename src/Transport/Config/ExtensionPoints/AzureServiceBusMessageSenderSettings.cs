namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusMessageSenderSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusMessageSenderSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }
        
        public AzureServiceBusMessageSenderSettings RetryPolicy(RetryPolicy retryPolicy)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryPolicy, retryPolicy);

            return this;
        }

        public AzureServiceBusMessageSenderSettings BackOffTimeOnThrottle(TimeSpan backoffTime)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageSenders.BackOffTimeOnThrottle, backoffTime);

            return this;
        }

        public AzureServiceBusMessageSenderSettings RetryAttemptsOnThrottle(int count)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryAttemptsOnThrottle, count);

            return this;
        }

        public AzureServiceBusMessageSenderSettings MaximuMessageSizeInKilobytes(int sizeInKilobytes)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximuMessageSizeInKilobytes, sizeInKilobytes);

            return this;
        }
    }
}