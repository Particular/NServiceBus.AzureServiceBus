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
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out namespaces) || namespaces.Count != 2)
            {
                throw new ConfigurationErrorsException("The 'FailOver' namespace partitioning strategy requires exactly two namespaces to be configured");
            }
        }

        public FailOverMode Mode { get; set; }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(string endpointName, PartitioningIntent partitioningIntent)
        {
            var primary = namespaces.First();
            var secondary = namespaces.Last();

            if (partitioningIntent == PartitioningIntent.Sending)
            {
                yield return Mode == FailOverMode.Primary
                ? new RuntimeNamespaceInfo(primary.Name, primary.ConnectionString, NamespaceMode.Active)
                : new RuntimeNamespaceInfo(secondary.Name, secondary.ConnectionString, NamespaceMode.Active);
            }

            if (partitioningIntent == PartitioningIntent.Creating || partitioningIntent == PartitioningIntent.Receiving)
            {
                yield return new RuntimeNamespaceInfo(primary.Name, primary.ConnectionString, Mode == FailOverMode.Primary ? NamespaceMode.Active : NamespaceMode.Passive);
                yield return new RuntimeNamespaceInfo(secondary.Name, secondary.ConnectionString, Mode == FailOverMode.Secondary ? NamespaceMode.Active : NamespaceMode.Passive);
            }
        }
    }
}