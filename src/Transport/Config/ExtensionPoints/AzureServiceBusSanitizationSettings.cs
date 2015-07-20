namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusSanitizationSettings : ExposeSettings
    {
        public AzureServiceBusSanitizationSettings(SettingsHolder settings)
            : base(settings)
        {
        }
    }
}