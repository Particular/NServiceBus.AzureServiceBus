namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NServiceBus.Settings;

    public class SingleNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        private readonly NamespaceConfigurations _namespaces;

        public SingleNamespacePartitioningStrategy(ReadOnlySettings settings)
        {
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out _namespaces) || _namespaces.Count != 1)
            {
                throw new ConfigurationErrorsException("The 'Single' namespace partitioning strategy requires exactly one namespace, please configure the connection string to your azure servicebus namespace");
            }
        }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(string endpointName, PartitioningIntent partitioningIntent)
        {
            var @namespace = _namespaces.First();
            yield return new RuntimeNamespaceInfo(@namespace.Name, @namespace.ConnectionString, NamespaceMode.Active);
        }
    }
}