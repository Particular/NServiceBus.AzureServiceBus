namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Settings;

    public class ShardedNamespacePartitioning : INamespacePartitioningStrategy
    {
        NamespaceConfigurations namespaces;
        Func<int> shardingRule;

        public ShardedNamespacePartitioning(ReadOnlySettings settings)
        {
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out namespaces) || namespaces.Count <= 1)
            {
                throw new ConfigurationErrorsException("The 'Sharded' namespace partitioning strategy requires more than one namespace, please configure multiple azure servicebus namespaces");
            }
        }

        public void SetShardingRule(Func<int> rule)
        {
            shardingRule = rule;
        }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(string endpointname, PartitioningIntent partitioningIntent)
        {
            if (shardingRule == null)
            {
                throw new ConfigurationErrorsException("The 'Sharded' namespace partitioning strategy requires a configured sharding rule to determine a namespace, please configure a sharding rule");
            }

            var index = shardingRule();

            if (partitioningIntent == PartitioningIntent.Sending)
            {
                var @namespace = namespaces.ElementAt(index);
                yield return new RuntimeNamespaceInfo(@namespace.Name, @namespace.ConnectionString, NamespaceMode.Active);
            }

            if (partitioningIntent == PartitioningIntent.Creating || partitioningIntent == PartitioningIntent.Receiving)
            {
                for (var i = 0; i < namespaces.Count; i++)
                {
                    var @namespace = namespaces.ElementAt(i);
                    yield return new RuntimeNamespaceInfo(@namespace.Name, @namespace.ConnectionString, i == index ? NamespaceMode.Active : NamespaceMode.Passive);
                }
            }
        }
    }
}