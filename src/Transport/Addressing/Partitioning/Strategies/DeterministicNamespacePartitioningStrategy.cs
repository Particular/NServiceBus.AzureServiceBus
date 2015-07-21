namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    public class DeterministicNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        readonly IList<string> _connectionstrings;
        Func<int> _allocationRule;

        public DeterministicNamespacePartitioningStrategy(IList<string> connectionstrings)
        {
            if (connectionstrings.Count <= 1)
            {
                throw new ConfigurationErrorsException("The 'Deterministic' namespace partitioning strategy requires more than one namespace, please configure multiple azure servicebus namespaces");
            }

            _connectionstrings = connectionstrings;
        }

        public void SetAllocationRule(Func<int> rule)
        {
            _allocationRule = rule;
        }

        public string GetConnectionString(string endpointName)
        {
            if (_allocationRule ==  null)
            {
                throw new ConfigurationErrorsException("The 'Deterministic' namespace partitioning strategy requires a configured allocation rule to determine a namespace, please configure an allocation rule");
            }

            return _connectionstrings[_allocationRule()];
        }
    }
}