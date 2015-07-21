namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    public class SingleNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        readonly string _connectionstring;

        public SingleNamespacePartitioningStrategy(IReadOnlyCollection<string> connectionstrings)
        {
            if (connectionstrings.Count == 0)
            {
                throw new ConfigurationErrorsException("The 'Single' namespace partitioning strategy requires exactly one namespace, please configure the connection string to your azure servicebus namespace");
            }

            if (connectionstrings.Count > 1)
            {
                throw new ConfigurationErrorsException("The 'Single' namespace partitioning strategy does not support multiple namespaces");
            }

            _connectionstring = connectionstrings.First();
        }

        public string GetConnectionString(string endpointName)
        {
            return _connectionstring;
        }
    }
}