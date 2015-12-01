namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusTransactionSettings : ExposeSettings
    {
        public AzureServiceBusTransactionSettings(SettingsHolder settings)
            : base(settings)
        {
        }
    }
}