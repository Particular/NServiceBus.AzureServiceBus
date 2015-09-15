namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using NServiceBus.Settings;

    public class ShardedNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        readonly IList<string> _connectionstrings;
        Func<int> _shardingRule;

        public ShardedNamespacePartitioningStrategy(ReadOnlySettings settings)
        {
            List<string> connectionstrings;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out connectionstrings))
            {
                connectionstrings = new List<string>();
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

        public IEnumerable<NamespaceInfo> GetNamespaces(string endpointname, Purpose purpose)
        {
            if (_shardingRule ==  null)
            {
                throw new ConfigurationErrorsException("The 'Sharded' namespace partitioning strategy requires a configured sharding rule to determine a namespace, please configure a sharding rule");
            }

            var index = _shardingRule();

            if (purpose == Purpose.Sending)
            {
                yield return new NamespaceInfo(_connectionstrings[index], NamespaceMode.Active);
            }

            if (purpose == Purpose.Creating || purpose == Purpose.Receiving)
            {
                for (var i = 0; i < _connectionstrings.Count; i++)
                {
                    yield return new NamespaceInfo(_connectionstrings[i], i == index ? NamespaceMode.Active : NamespaceMode.Passive);
                }
            }
        }
    }
}