namespace NServiceBus
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Settings;
    using Transport.AzureServiceBus;

    /// <summary>
    /// Strategy using primary and secondary namespaces. The secondary namespace is used as a fallback in case of problems with the primary namespace.
    /// <remarks>Assumes all entities are in the primary and secondary namespaces, where only the primary is in use by default.</remarks>
    /// </summary>
    public class FailOverNamespacePartitioning : INamespacePartitioningStrategy
    {
        internal FailOverNamespacePartitioning(ReadOnlySettings settings)
        {
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, out NamespaceConfigurations namespaces))
            {
                throw new ConfigurationErrorsException($"The '{nameof(FailOverNamespacePartitioning)}' strategy requires exactly two namespaces to be configured, please use {nameof(AzureServiceBusTransportExtensions.NamespacePartitioning)}().{nameof(AzureServiceBusNamespacePartitioningSettings.AddNamespace)}() to register the namespaces.");
            }

            namespaces = new NamespaceConfigurations(namespaces.Where(n => n.Purpose == NamespacePurpose.Partitioning).ToList());

            if (namespaces.Count < 2)
            {
                throw new ConfigurationErrorsException($"The '{nameof(FailOverNamespacePartitioning)}' strategy requires exactly two namespaces for the purpose of partitioning, found {namespaces.Count}, please use {nameof(AzureServiceBusTransportExtensions.NamespacePartitioning)}().{nameof(AzureServiceBusNamespacePartitioningSettings.AddNamespace)}() to register additional namespaces.");
            }
            if (namespaces.Count > 2)
            {
                throw new ConfigurationErrorsException($"The '{nameof(FailOverNamespacePartitioning)}' strategy requires exactly two namespaces for the purpose of partitioning, found {namespaces.Count}, please register less namespaces.");
            }

            var primary = namespaces.First();
            var secondary = namespaces.Last();

            runtimeNamespaces = new[]
            {
                new RuntimeNamespaceInfo(primary.Alias, primary.Connection, primary.Purpose, Mode == FailOverMode.Primary ? NamespaceMode.Active : NamespaceMode.Passive),
                new RuntimeNamespaceInfo(secondary.Alias, secondary.Connection, secondary.Purpose, Mode == FailOverMode.Secondary ? NamespaceMode.Active : NamespaceMode.Passive)
            };

            SendingNamespacesCanBeCached = true;
        }

        /// <summary>Current mode.</summary>
        public FailOverMode Mode { get; set; }

        /// <summary>
        /// Gets whether the information returned by the strategy for <see cref="PartitioningIntent.Sending"/> is cache-able.
        /// </summary>
        public bool SendingNamespacesCanBeCached { get; }

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