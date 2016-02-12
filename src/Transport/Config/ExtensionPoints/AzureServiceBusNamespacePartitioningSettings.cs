namespace NServiceBus
{
    using System.Collections.Generic;
    using NServiceBus.AzureServiceBus;
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
        }

        public AzureServiceBusNamespacePartitioningSettings UseStrategy<T>() where T : INamespacePartitioningStrategy
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, typeof(T));

            return this;
        }

        public AzureServiceBusNamespacePartitioningSettings AddNamespace(string name, string connectionString)
        {
            Dictionary<string, string> namespaces;
            if (_settings.HasExplicitValue(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces))
            {
                namespaces = _settings.Get<Dictionary<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            }
            else
            {
                namespaces = new Dictionary<string, string>();
                _settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, namespaces);
            }
            
            namespaces.Add(name, connectionString);
            return this;
        }
    }
}