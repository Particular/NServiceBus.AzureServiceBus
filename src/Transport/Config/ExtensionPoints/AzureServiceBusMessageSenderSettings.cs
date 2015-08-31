namespace NServiceBus
{
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
    }
}