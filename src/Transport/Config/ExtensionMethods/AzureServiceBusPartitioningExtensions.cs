namespace NServiceBus
{
    using System.Collections.Generic;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusPartitioningExtensions
    {
        public static AzureServiceBusPartitioningSettings Strategy<T>(this AzureServiceBusPartitioningSettings partitioningSettings) where T : IPartitioningStrategy
        {
            partitioningSettings.GetSettings().Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, typeof(T));

            return partitioningSettings;
        }

        public static AzureServiceBusPartitioningSettings AddNamespace(this AzureServiceBusPartitioningSettings partitioningSettings, string @namespace)
        {
            var namespaces = partitioningSettings.GetSettings().Get<List<string>>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespaces.Add(@namespace);
            return partitioningSettings;
        }

    }
}