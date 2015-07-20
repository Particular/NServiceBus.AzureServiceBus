namespace NServiceBus
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusValidationSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusValidationSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        public AzureServiceBusValidationSettings Strategy<T>() where T : IValidationStrategy
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy, typeof(T));

            return this;
        }
    }
}