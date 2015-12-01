namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusAddressingSettings : ExposeSettings
    {
        public AzureServiceBusAddressingSettings(SettingsHolder settings)
            : base(settings)
        {
        }
    }
}
