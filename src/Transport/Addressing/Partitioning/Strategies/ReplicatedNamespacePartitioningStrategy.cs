namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NServiceBus.Settings;

    public class ReplicatedNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        private readonly NamespaceConfigurations _namespaces;

        public ReplicatedNamespacePartitioningStrategy(ReadOnlySettings settings)
        {
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out _namespaces) || _namespaces.Count <= 1)
            {
                throw new ConfigurationErrorsException("The 'Replicated' namespace partitioning strategy requires more than one namespace, please configure additional connection strings");
            }
        }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(string endpointName, PartitioningIntent partitioningIntent)
        {
            return _namespaces.Select(x => new RuntimeNamespaceInfo(x.Name, x.ConnectionString, NamespaceMode.Active));
        }
    }
}