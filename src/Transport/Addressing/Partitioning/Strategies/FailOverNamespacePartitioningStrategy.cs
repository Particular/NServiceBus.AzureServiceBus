namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    public class FailOverNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        readonly IReadOnlyCollection<string> _connectionstrings;

        public FailOverNamespacePartitioningStrategy(IReadOnlyCollection<string> connectionstrings)
        {
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