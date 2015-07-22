namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Configuration;

    public class ReplicatedNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        readonly IReadOnlyCollection<string> _connectionstrings;

        public ReplicatedNamespacePartitioningStrategy(IReadOnlyCollection<string> connectionstrings)
        {
            if (connectionstrings.Count <= 1)
            {
                throw new ConfigurationErrorsException("The 'Replicated' namespace partitioning strategy requires more than one namespace, please configure additional connection strings");
            }
            
            _connectionstrings = connectionstrings;
        }

        public IEnumerable<string> GetConnectionStrings(string endpointName)
        {
            return _connectionstrings;
        }
    }
}