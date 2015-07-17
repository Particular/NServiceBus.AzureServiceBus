namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusCompositionSettings : ExposeSettings
    {
        public AzureServiceBusCompositionSettings(SettingsHolder settings)
            : base(settings)
        {
        }
    }
}