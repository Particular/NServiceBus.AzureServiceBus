namespace NServiceBus
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Settings;
    using Transport.AzureServiceBus;

    public class FailOverNamespacePartitioning : INamespacePartitioningStrategy
    {
        internal FailOverNamespacePartitioning(ReadOnlySettings settings)
        {
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, out namespaces))
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
        }

        public FailOverMode Mode { get; set; }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent)
        {
            var primary = namespaces.First();
            var secondary = namespaces.Last();

            yield return new RuntimeNamespaceInfo(primary.Alias, primary.Connection, primary.Purpose, Mode == FailOverMode.Primary ? NamespaceMode.Active : NamespaceMode.Passive);
            yield return new RuntimeNamespaceInfo(secondary.Alias, secondary.Connection, secondary.Purpose, Mode == FailOverMode.Secondary ? NamespaceMode.Active : NamespaceMode.Passive);
        }

        NamespaceConfigurations namespaces;
    }
}