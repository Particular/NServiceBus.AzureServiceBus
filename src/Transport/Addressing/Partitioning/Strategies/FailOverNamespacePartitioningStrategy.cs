namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NServiceBus.Settings;

    public class FailOverNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        readonly IReadOnlyCollection<string> _connectionstrings;

        public FailOverNamespacePartitioningStrategy(ReadOnlySettings settings)
        {
            List<string> connectionstrings;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out connectionstrings))
            {
                connectionstrings = new List<string>();
            }

            if (connectionstrings.Count != 2)
            {
                throw new ConfigurationErrorsException("The 'FailOver' namespace partitioning strategy requires exactly two namespaces to be configured");
            }

            _connectionstrings = connectionstrings;
        }

        public FailOverMode Mode { get; set; }

        public IEnumerable<NamespaceInfo> GetNamespaces(string endpointName, PartitioningIntent partitioningIntent)
        {
            if(partitioningIntent == PartitioningIntent.Sending)
            {
                yield return Mode == FailOverMode.Primary 
                ? new NamespaceInfo(_connectionstrings.First(), NamespaceMode.Active)
                : new NamespaceInfo(_connectionstrings.Last(), NamespaceMode.Active);
            }

            if (partitioningIntent == PartitioningIntent.Creating || partitioningIntent == PartitioningIntent.Receiving)
            {
                yield return new NamespaceInfo(_connectionstrings.First(), Mode == FailOverMode.Primary ? NamespaceMode.Active : NamespaceMode.Passive);
                yield return new NamespaceInfo(_connectionstrings.Last(), Mode == FailOverMode.Secondary ? NamespaceMode.Active : NamespaceMode.Passive);
            }
        }

    }

    public enum FailOverMode
    {
        Primary,
        Secondary
    }
}