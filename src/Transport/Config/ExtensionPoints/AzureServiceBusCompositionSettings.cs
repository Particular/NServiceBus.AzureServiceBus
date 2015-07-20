namespace NServiceBus
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusCompositionSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusCompositionSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        public AzureServiceBusCompositionSettings Strategy<T>() where T : ICompositionStrategy
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy, typeof(T));

            return this;
        }
    }
}