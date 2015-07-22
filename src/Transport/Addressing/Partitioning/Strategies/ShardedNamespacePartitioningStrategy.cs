namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    public class ShardedNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        readonly IList<string> _connectionstrings;
        Func<int> _shardingRule;

        public ShardedNamespacePartitioningStrategy(IList<string> connectionstrings)
        {
            if (connectionstrings.Count <= 1)
            {
                throw new ConfigurationErrorsException("The 'Sharded' namespace partitioning strategy requires more than one namespace, please configure multiple azure servicebus namespaces");
            }

            _connectionstrings = connectionstrings;
        }

        public void SetShardingRule(Func<int> rule )
        {
            _shardingRule = rule;
        }

        public IEnumerable<string> GetConnectionStrings(string endpointname)
        {
            if (_shardingRule ==  null)
            {
                throw new ConfigurationErrorsException("The 'Sharded' namespace partitioning strategy requires a configured sharding rule to determine a namespace, please configure a sharding rule");
            }

            yield return _connectionstrings[_shardingRule()];
        }
    }
}