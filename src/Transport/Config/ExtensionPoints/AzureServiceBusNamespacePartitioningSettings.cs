namespace NServiceBus
{
    using System.Collections.Generic;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusNamespacePartitioningSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusNamespacePartitioningSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
            _settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, new List<string>());
        }

        public AzureServiceBusNamespacePartitioningSettings Strategy<T>() where T : INamespacePartitioningStrategy
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, typeof(T));

            return this;
        }

        public AzureServiceBusNamespacePartitioningSettings AddNamespace(string @namespace)
        {
            var namespaces = _settings.Get<List<string>>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespaces.Add(@namespace);
            return this;
        }
    }
}