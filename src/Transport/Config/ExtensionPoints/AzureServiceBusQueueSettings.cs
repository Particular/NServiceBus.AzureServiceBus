namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusQueueSettings : ExposeSettings
    {
        public AzureServiceBusQueueSettings(SettingsHolder settings)
            : base(settings)
        {
        }
    }
}