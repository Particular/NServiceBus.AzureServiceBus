namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Settings;

    public class ReplicatedNamespacePartitioning : INamespacePartitioningStrategy
    {
        NamespaceConfigurations namespaces;

        public ReplicatedNamespacePartitioning(ReadOnlySettings settings)
        {
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out namespaces) || namespaces.Count <= 1)
            {
                throw new ConfigurationErrorsException("The 'Replicated' namespace partitioning strategy requires more than one namespace, please configure additional connection strings");
            }
        }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent)
        {
            return namespaces.Select(x => new RuntimeNamespaceInfo(x.Name, x.ConnectionString, NamespaceMode.Active));
        }
    }
}