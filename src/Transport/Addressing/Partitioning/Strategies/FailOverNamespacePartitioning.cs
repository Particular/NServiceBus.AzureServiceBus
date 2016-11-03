namespace NServiceBus
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using AzureServiceBus.Addressing.Partitioning;
    using Transport.AzureServiceBus;
    using Settings;

    public class FailOverNamespacePartitioning : INamespacePartitioningStrategy
    {
        List<RuntimeNamespaceInfo> namespaces;

        public FailOverNamespacePartitioning(ReadOnlySettings settings)
        {
            namespaces = settings.GetPartitioningNamespaces();

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

            if (partitioningIntent == PartitioningIntent.Sending)
            {
                yield return Mode == FailOverMode.Primary ? primary : secondary;
            }

            if (partitioningIntent == PartitioningIntent.Creating || partitioningIntent == PartitioningIntent.Receiving)
            {
                yield return new RuntimeNamespaceInfo(primary.Alias, primary.ConnectionString, NamespacePurpose.Partitioning, Mode == FailOverMode.Primary ? NamespaceMode.Active : NamespaceMode.Passive);
                yield return new RuntimeNamespaceInfo(secondary.Alias, secondary.ConnectionString, NamespacePurpose.Partitioning, Mode == FailOverMode.Secondary ? NamespaceMode.Active : NamespaceMode.Passive);
            }
        }
    }
}