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

        public IEnumerable<string> GetConnectionStrings(string endpointName)
        {
            yield return Mode == FailOverMode.Primary ? _connectionstrings.First() : _connectionstrings.Last();
        }
    }

    public enum FailOverMode
    {
        Primary,
        Secondary
    }
}