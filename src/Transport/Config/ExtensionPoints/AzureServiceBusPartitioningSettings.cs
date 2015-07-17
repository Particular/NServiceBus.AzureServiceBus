namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusPartitioningSettings : ExposeSettings
    {
        public AzureServiceBusPartitioningSettings(SettingsHolder settings)
            : base(settings)
        {
        }
    }
}