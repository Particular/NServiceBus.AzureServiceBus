namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Settings;

    public class FailOverNamespacePartitioning : INamespacePartitioningStrategy
    {
        NamespaceConfigurations namespaces;

        public FailOverNamespacePartitioning(ReadOnlySettings settings)
        {
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, out namespaces))
            {
                throw new ConfigurationErrorsException($"The '{nameof(FailOverNamespacePartitioning)}' strategy requires exactly two namespaces to be configured, please use {nameof(AzureServiceBusTransportExtensions.NamespacePartitioning)}().{nameof(AzureServiceBusNamespacePartitioningSettings.AddNamespace)}() to register the namespaces");
            }

            namespaces = new NamespaceConfigurations(namespaces.Where(n => n.Purpose == NamespacePurpose.Partitioning).ToList());

            if (namespaces.Count < 2)
            {
                throw new ConfigurationErrorsException($"The '{nameof(FailOverNamespacePartitioning)}' strategy requires exactly two namespaces for the purpose of partitioning, found {namespaces.Count}, please use {nameof(AzureServiceBusTransportExtensions.NamespacePartitioning)}().{nameof(AzureServiceBusNamespacePartitioningSettings.AddNamespace)}() to register additional namespaces");
            }
            if (namespaces.Count > 2)
            {
                throw new ConfigurationErrorsException($"The '{nameof(FailOverNamespacePartitioning)}' strategy requires exactly two namespaces for the purpose of partitioning, found {namespaces.Count}, please register less namespaces");
            }
        }

        public FailOverMode Mode { get; set; }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent)
        {
            var primary = namespaces.First();
            var secondary = namespaces.Last();

            if (partitioningIntent == PartitioningIntent.Sending)
            {
                yield return Mode == FailOverMode.Primary
                ? new RuntimeNamespaceInfo(primary.Name, primary.ConnectionString, primary.Purpose, NamespaceMode.Active)
                : new RuntimeNamespaceInfo(secondary.Name, secondary.ConnectionString, secondary.Purpose, NamespaceMode.Active);
            }

            if (partitioningIntent == PartitioningIntent.Creating || partitioningIntent == PartitioningIntent.Receiving)
            {
                yield return new RuntimeNamespaceInfo(primary.Name, primary.ConnectionString, primary.Purpose, Mode == FailOverMode.Primary ? NamespaceMode.Active : NamespaceMode.Passive);
                yield return new RuntimeNamespaceInfo(secondary.Name, secondary.ConnectionString, secondary.Purpose, Mode == FailOverMode.Secondary ? NamespaceMode.Active : NamespaceMode.Passive);
            }
        }
    }
}