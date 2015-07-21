namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Configuration;

    public class RoundRobinNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        readonly CircularBuffer<string> _connectionstrings;

        public RoundRobinNamespacePartitioningStrategy(IReadOnlyCollection<string> connectionstrings)
        {
            if (connectionstrings.Count <= 1)
            {
                throw new ConfigurationErrorsException("The 'RoundRobin' namespace partitioning strategy requires more than one namespace, please configure multiple azure servicebus namespaces");
            }

            _connectionstrings = new CircularBuffer<string>(connectionstrings.Count);
            foreach (var connectionstring in connectionstrings)
            {
                _connectionstrings.Put(connectionstring);
            }
        }

        public string GetConnectionString(string endpointName)
        {
            return _connectionstrings.Get();
        }
    }
}