namespace NServiceBus
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Settings;
    using Transport.AzureServiceBus;

    /// <summary>
    /// Strategy for all entities in a single namespace.
    /// </summary>
    public class SingleNamespacePartitioning : INamespacePartitioningStrategy
    {
        internal SingleNamespacePartitioning(ReadOnlySettings settings)
        {
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, out NamespaceConfigurations namespaces))
            {
                throw new ConfigurationErrorsException($"The '{nameof(SingleNamespacePartitioning)}' strategy requires exactly one namespace, please configure the connection string to your azure servicebus namespace.");
            }

            namespaces = new NamespaceConfigurations(namespaces.Where(n => n.Purpose == NamespacePurpose.Partitioning).ToList());

            if (namespaces.Count != 1)
            {
                throw new ConfigurationErrorsException($"The '{nameof(SingleNamespacePartitioning)}' strategy requires exactly one namespace for the purpose of partitioning, found {namespaces.Count}. Please remove additional namespace registrations.");
            }

            var @namespace = namespaces.First();
            runtimeNamespaces = new[]
            {
                new RuntimeNamespaceInfo(@namespace.Alias, @namespace.Connection, @namespace.Purpose, NamespaceMode.Active)
            };
        }

        /// <summary>
        /// Return a set of namespaces required by strategy for <see cref="PartitioningIntent"/>.
        /// </summary>
        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent)
        {
            return runtimeNamespaces;
        }

        RuntimeNamespaceInfo[] runtimeNamespaces;
    }
}