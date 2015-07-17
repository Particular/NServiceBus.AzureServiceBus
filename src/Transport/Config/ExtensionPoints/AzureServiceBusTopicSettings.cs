namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusTopicSettings : ExposeSettings
    {
        public AzureServiceBusTopicSettings(SettingsHolder settings)
            : base(settings)
        {
        }
    }
}