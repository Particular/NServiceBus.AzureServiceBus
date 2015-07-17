namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusValidationSettings : ExposeSettings
    {
        public AzureServiceBusValidationSettings(SettingsHolder settings)
            : base(settings)
        {
        }
    }
}