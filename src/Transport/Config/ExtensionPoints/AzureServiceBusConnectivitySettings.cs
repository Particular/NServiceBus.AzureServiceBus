namespace NServiceBus
{
    using NServiceBus.AzureServiceBus;
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

        public AzureServiceBusConnectivitySettings NumberOfClientsPerEntity(int number)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity, number);

            return this;
        }

        public AzureServiceBusConnectivitySettings SendViaReceiveQueue(bool sendViaReceiveQueue)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue, sendViaReceiveQueue);

            return this;


        }
    }
}