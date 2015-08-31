namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusConnectivitySettingsExtensions
    {
        public static AzureServiceBusMessageReceiverSettings MessageReceivers(this AzureServiceBusConnectivitySettings azureServiceBusConnectivitySettings)
        {
            return new AzureServiceBusMessageReceiverSettings(azureServiceBusConnectivitySettings.GetSettings());
        }

        public static AzureServiceBusMessageSenderSettings MessageSenders(this AzureServiceBusConnectivitySettings azureServiceBusConnectivitySettings)
        {
            return new AzureServiceBusMessageSenderSettings(azureServiceBusConnectivitySettings.GetSettings());
        }

        public static AzureServiceBusMessagingFactoriesSettings MessagingFactories(this AzureServiceBusConnectivitySettings azureServiceBusConnectivitySettings)
        {
            return new AzureServiceBusMessagingFactoriesSettings(azureServiceBusConnectivitySettings.GetSettings());
        }

    }
}