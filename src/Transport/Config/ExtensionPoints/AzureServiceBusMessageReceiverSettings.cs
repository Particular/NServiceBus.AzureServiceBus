namespace NServiceBus
{
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusMessageReceiverSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusMessageReceiverSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        public AzureServiceBusMessageReceiverSettings ReceiveMode(ReceiveMode receiveMode)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode, receiveMode);

            return this;
        }

        public AzureServiceBusMessageReceiverSettings PrefetchCount(int prefetchCount)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount, prefetchCount);

            return this;
        }

        public AzureServiceBusMessageReceiverSettings RetryPolicy(RetryPolicy retryPolicy)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageReceivers.RetryPolicy, retryPolicy);

            return this;
        }
    }
}