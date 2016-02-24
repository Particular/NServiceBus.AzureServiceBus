namespace NServiceBus
{
    using System;
    using NServiceBus.AzureServiceBus;
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

        public AzureServiceBusAddressingSettings UseLogicalNamespaceName()
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Addressing.UseLogicalNamespaceName, (Func<NamespaceInfo, string>)(x => x.Name));
            return this;
        }
    }
}
