namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusSubscriptionSettings : ExposeSettings
    {
        public AzureServiceBusSubscriptionSettings(SettingsHolder settings)
            : base(settings)
        {
        }
    }
}