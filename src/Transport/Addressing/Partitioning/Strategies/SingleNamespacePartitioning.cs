namespace NServiceBus
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Settings;
    using Transport.AzureServiceBus;

    public class SingleNamespacePartitioning : INamespacePartitioningStrategy
    {
        NamespaceConfigurations namespaces;

        internal SingleNamespacePartitioning(ReadOnlySettings settings)
        {
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, out namespaces))
            {
                throw new ConfigurationErrorsException($"The '{nameof(SingleNamespacePartitioning)}' strategy requires exactly one namespace, please configure the connection string to your azure servicebus namespace.");
            }

            namespaces = new NamespaceConfigurations(namespaces.Where(n => n.Purpose == NamespacePurpose.Partitioning).ToList());

            if (namespaces.Count != 1)
            {
                throw new ConfigurationErrorsException($"The '{nameof(SingleNamespacePartitioning)}' strategy requires exactly one namespace for the purpose of partitioning, found {namespaces.Count}. Please remove additional namespace registrations.");
            }
        }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent)
        {
            var @namespace = namespaces.First();
            yield return new RuntimeNamespaceInfo(@namespace.Alias, @namespace.Connection, @namespace.Purpose, NamespaceMode.Active);
        }
    }
}