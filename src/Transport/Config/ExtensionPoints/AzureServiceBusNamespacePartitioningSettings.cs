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

        public AzureServiceBusNamespacePartitioningSettings AddNamespace(string @namespace)
        {
            List<string> namespaces;
            if (_settings.HasExplicitValue(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces))
            {
                namespaces = _settings.Get<List<string>>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            }
            else
            {
                namespaces = new List<string>();
                _settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, namespaces);
            }
            
            namespaces.Add(@namespace);
            return this;
        }
    }
}