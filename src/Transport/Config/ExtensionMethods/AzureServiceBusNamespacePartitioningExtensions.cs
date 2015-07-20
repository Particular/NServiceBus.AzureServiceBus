namespace NServiceBus
{
    using System.Collections.Generic;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusNamespacePartitioningExtensions
    {
        public static AzureServiceBusNamespacePartitioningSettings Strategy<T>(this AzureServiceBusNamespacePartitioningSettings namespacePartitioningSettings) where T : INamespacePartitioningStrategy
        {
            namespacePartitioningSettings.GetSettings().Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, typeof(T));

            return namespacePartitioningSettings;
        }

        public static AzureServiceBusNamespacePartitioningSettings AddNamespace(this AzureServiceBusNamespacePartitioningSettings namespacePartitioningSettings, string @namespace)
        {
            var innerSettings = namespacePartitioningSettings.GetSettings();

            if (!innerSettings.HasSetting(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces))
            {
                innerSettings.Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, new List<string>());
            }

            var namespaces = innerSettings.Get<List<string>>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespaces.Add(@namespace);
            return namespacePartitioningSettings;
        }

    }
}