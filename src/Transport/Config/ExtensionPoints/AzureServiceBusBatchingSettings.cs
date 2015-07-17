namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusBatchingSettings : ExposeSettings
    {
        public AzureServiceBusBatchingSettings(SettingsHolder settings)
            : base(settings)
        {
        }
    }
}