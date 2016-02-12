namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NServiceBus.Settings;

    public class ShardedNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        readonly Dictionary<string, string> _connectionstrings;
        Func<int> _shardingRule;

        public ShardedNamespacePartitioningStrategy(ReadOnlySettings settings)
        {
            Dictionary<string, string> connectionstrings;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out connectionstrings))
            {
                connectionstrings = new Dictionary<string, string>();
            }

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

        public IEnumerable<NamespaceInfo> GetNamespaces(string endpointname, PartitioningIntent partitioningIntent)
        {
            if (_shardingRule ==  null)
            {
                throw new ConfigurationErrorsException("The 'Sharded' namespace partitioning strategy requires a configured sharding rule to determine a namespace, please configure a sharding rule");
            }

            var index = _shardingRule();

            if (partitioningIntent == PartitioningIntent.Sending)
            {
                var connectionstring = _connectionstrings.ElementAt(index);
                yield return new NamespaceInfo(connectionstring.Key, connectionstring.Value, NamespaceMode.Active);
            }

            if (partitioningIntent == PartitioningIntent.Creating || partitioningIntent == PartitioningIntent.Receiving)
            {
                for (var i = 0; i < _connectionstrings.Count; i++)
                {
                    var connectionstring = _connectionstrings.ElementAt(i);
                    yield return new NamespaceInfo(connectionstring.Key, connectionstring.Value, i == index ? NamespaceMode.Active : NamespaceMode.Passive);
                }
            }
        }
    }
}