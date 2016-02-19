namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NServiceBus.Settings;

    public class SingleNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        private readonly NamespacesDefinition _namespaces;

        public SingleNamespacePartitioningStrategy(ReadOnlySettings settings)
        {
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out _namespaces) || _namespaces.Count != 1)
            {
                throw new ConfigurationErrorsException("The 'Single' namespace partitioning strategy requires exactly one namespace, please configure the connection string to your azure servicebus namespace");
            }
        }

        public IEnumerable<NamespaceInfo> GetNamespaces(string endpointName, PartitioningIntent partitioningIntent)
        {
            var definition = _namespaces.First();
            yield return new NamespaceInfo(definition.ConnectionString, NamespaceMode.Active);
        }
    }
}