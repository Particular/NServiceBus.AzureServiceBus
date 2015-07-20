namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusNamespacePartitioningSettings : ExposeSettings
    {
        public AzureServiceBusNamespacePartitioningSettings(SettingsHolder settings)
            : base(settings)
        {
        }
    }
}