namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NServiceBus.Settings;

    public class SingleNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        readonly KeyValuePair<string, string> _connectionstring;

        public SingleNamespacePartitioningStrategy(ReadOnlySettings settings)
        {
            Dictionary<string, string> connectionstrings;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out connectionstrings))
            {
                connectionstrings = new Dictionary<string, string>();
            }

            if (connectionstrings.Count == 0)
            {
                throw new ConfigurationErrorsException("The 'Single' namespace partitioning strategy requires exactly one namespace, please configure the connection string to your azure servicebus namespace");
            }

            if (connectionstrings.Count > 1)
            {
                throw new ConfigurationErrorsException("The 'Single' namespace partitioning strategy does not support multiple namespaces");
            }

            _connectionstring = connectionstrings.First();
        }

        public IEnumerable<NamespaceInfo> GetNamespaces(string endpointName, PartitioningIntent partitioningIntent)
        {
            yield return new NamespaceInfo(_connectionstring.Key, _connectionstring.Value, NamespaceMode.Active);
        }
    }
}