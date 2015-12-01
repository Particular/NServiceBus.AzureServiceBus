namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusResourceSettings : ExposeSettings
    {

        public AzureServiceBusResourceSettings(SettingsHolder settings)
            : base(settings)
        {
        }

    }
}
