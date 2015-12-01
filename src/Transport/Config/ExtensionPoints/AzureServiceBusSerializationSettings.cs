namespace NServiceBus
{
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusSerializationSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusSerializationSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        public AzureServiceBusSerializationSettings BrokeredMessageBodyType(SupportedBrokeredMessageBodyTypes type)
        {
            _settings.Set(WellKnownConfigurationKeys.Serialization.BrokeredMessageBodyType, type);

            return this;
        }
    }
}