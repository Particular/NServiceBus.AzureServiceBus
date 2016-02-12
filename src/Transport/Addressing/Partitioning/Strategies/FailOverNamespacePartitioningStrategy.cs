namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NServiceBus.Settings;

    public class FailOverNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        readonly Dictionary<string, string> _connectionstrings;

        public FailOverNamespacePartitioningStrategy(ReadOnlySettings settings)
        {
            Dictionary<string, string> connectionstrings;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out connectionstrings))
            {
                connectionstrings = new Dictionary<string, string>();
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
                ? new NamespaceInfo(_connectionstrings.First().Key, _connectionstrings.First().Value, NamespaceMode.Active)
                : new NamespaceInfo(_connectionstrings.Last().Key, _connectionstrings.Last().Value, NamespaceMode.Active);
            }

            if (partitioningIntent == PartitioningIntent.Creating || partitioningIntent == PartitioningIntent.Receiving)
            {
                yield return new NamespaceInfo(_connectionstrings.First().Key, _connectionstrings.First().Value, Mode == FailOverMode.Primary ? NamespaceMode.Active : NamespaceMode.Passive);
                yield return new NamespaceInfo(_connectionstrings.Last().Key, _connectionstrings.Last().Value, Mode == FailOverMode.Secondary ? NamespaceMode.Active : NamespaceMode.Passive);
            }
        }

    }

    public enum FailOverMode
    {
        Primary,
        Secondary
    }
}