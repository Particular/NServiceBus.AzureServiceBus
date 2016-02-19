namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NServiceBus.Settings;

    public class ShardedNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        private readonly NamespacesDefinition _namespaces;
        private Func<int> _shardingRule;

        public ShardedNamespacePartitioningStrategy(ReadOnlySettings settings)
        {
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out _namespaces) || _namespaces.Count <= 1)
            {
                throw new ConfigurationErrorsException("The 'Sharded' namespace partitioning strategy requires more than one namespace, please configure multiple azure servicebus namespaces");
            }
        }

        public void SetShardingRule(Func<int> rule)
        {
            _shardingRule = rule;
        }

        public IEnumerable<NamespaceInfo> GetNamespaces(string endpointname, PartitioningIntent partitioningIntent)
        {
            if (_shardingRule == null)
            {
                throw new ConfigurationErrorsException("The 'Sharded' namespace partitioning strategy requires a configured sharding rule to determine a namespace, please configure a sharding rule");
            }

            var index = _shardingRule();

            if (partitioningIntent == PartitioningIntent.Sending)
            {
                var namespaceDefinition = _namespaces.ElementAt(index);
                yield return new NamespaceInfo(namespaceDefinition.ConnectionString, NamespaceMode.Active);
            }

            if (partitioningIntent == PartitioningIntent.Creating || partitioningIntent == PartitioningIntent.Receiving)
            {
                for (var i = 0; i < _namespaces.Count; i++)
                {
                    var namespaceDefinition = _namespaces.ElementAt(i);
                    yield return new NamespaceInfo(namespaceDefinition.ConnectionString, i == index ? NamespaceMode.Active : NamespaceMode.Passive);
                }
            }
        }
    }
}