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

        public AzureServiceBusConnectivitySettings SendViaReceiveQueueIfPossible(bool sendViaReceiveQueueIfPossible)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueueIfPossible, sendViaReceiveQueueIfPossible);

            return this;


        }
    }
}