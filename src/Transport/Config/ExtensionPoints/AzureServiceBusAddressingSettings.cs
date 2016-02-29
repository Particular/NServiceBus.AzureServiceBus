namespace NServiceBus
{
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Topology.MetaModel;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusAddressingSettings : ExposeSettings
    {
        private readonly SettingsHolder _settings;

        public AzureServiceBusAddressingSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        public AzureServiceBusAddressingSettings UseNamespaceNamesInsteadOfConnectionStrings()
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Addressing.UseNamespaceNamesInsteadOfConnectionStrings, typeof(PassThroughNamespaceNameToConnectionStringMapper));
            return this;
        }
    }
}
