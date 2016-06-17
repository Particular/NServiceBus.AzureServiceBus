namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Settings;

    public class SingleNamespacePartitioning : INamespacePartitioningStrategy
    {
        NamespaceConfigurations namespaces;

        public SingleNamespacePartitioning(ReadOnlySettings settings)
        {
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out namespaces))
            {
                throw new ConfigurationErrorsException("The 'Single' namespace partitioning strategy requires exactly one namespace, please configure the connection string to your azure servicebus namespace");
            }

            namespaces = new NamespaceConfigurations(namespaces.Where(n => n.Purpose == NamespacePurpose.Partitioning).ToList());

            if (namespaces.Count != 1)
            {
                throw new ConfigurationErrorsException("The 'Single' namespace partitioning strategy requires exactly one   the purpose of configuring, found {namespaces.Count}. Please configure the connection string to your azure servicebus namespace");
            }
        }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent)
        {
            var @namespace = namespaces.First();
            yield return new RuntimeNamespaceInfo(@namespace.Name, @namespace.ConnectionString, @namespace.Purpose, NamespaceMode.Active);
        }
    }
}