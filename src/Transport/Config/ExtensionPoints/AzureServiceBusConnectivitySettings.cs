namespace NServiceBus
{
    using Microsoft.ServiceBus;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusConnectivitySettings : ExposeSettings
    {
        SettingsHolder settings;

        public AzureServiceBusConnectivitySettings(SettingsHolder settings) : base(settings)
        {
            this.settings = settings;
        }

        public AzureServiceBusConnectivitySettings NumberOfClientsPerEntity(int number)
        {
            settings.Set(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity, number);

            return this;
        }

        public AzureServiceBusConnectivitySettings SendViaReceiveQueue(bool sendViaReceiveQueue)
        {
            settings.Set(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue, sendViaReceiveQueue);

            return this;
        }
        public AzureServiceBusConnectivitySettings ConnectivityMode(ConnectivityMode connectivityMode)
        {
            settings.Set(WellKnownConfigurationKeys.Connectivity.ConnectivityMode, connectivityMode);

            return this;
        }
    }
}