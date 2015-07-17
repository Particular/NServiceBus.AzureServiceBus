namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusIndividualizationSettings : ExposeSettings
    {
        public AzureServiceBusIndividualizationSettings(SettingsHolder settings)
            : base(settings)
        {
        }
    }
}