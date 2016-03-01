namespace NServiceBus
{
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
            NamespaceConfigurations namespaces;
            if (!_settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out namespaces))
            {
                namespaces = new NamespaceConfigurations();
                _settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, namespaces);
            }
            
            namespaces.Add(name, connectionString);
            return this;
        }
    }
}