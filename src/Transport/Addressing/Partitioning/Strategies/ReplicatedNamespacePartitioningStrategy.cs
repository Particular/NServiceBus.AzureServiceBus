namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NServiceBus.Settings;

    public class ReplicatedNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        readonly Dictionary<string, string> _connectionstrings;

        public ReplicatedNamespacePartitioningStrategy(ReadOnlySettings settings)
        {
            Dictionary<string, string> connectionstrings;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out connectionstrings))
            {
                connectionstrings = new Dictionary<string, string>();
            }

            if (connectionstrings.Count <= 1)
            {
                throw new ConfigurationErrorsException("The 'Replicated' namespace partitioning strategy requires more than one namespace, please configure additional connection strings");
            }
            
            _connectionstrings = connectionstrings;
        }

        public IEnumerable<NamespaceInfo> GetNamespaces(string endpointName, PartitioningIntent partitioningIntent)
        {
            return _connectionstrings.Select(connectionstring => new NamespaceInfo(connectionstring.Key, connectionstring.Value, NamespaceMode.Active));
        }
    }
}