namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NServiceBus.Settings;

    public class RoundRobinNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        private readonly CircularBuffer<NamespaceInfo> _namespaces;

        public RoundRobinNamespacePartitioningStrategy(ReadOnlySettings settings)
        {
            NamespaceConfigurations namespaces;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out namespaces) || namespaces.Count <= 1)
            {
                throw new ConfigurationErrorsException("The 'RoundRobin' namespace partitioning strategy requires more than one namespace, please configure multiple azure servicebus namespaces");
            }

            _namespaces = new CircularBuffer<NamespaceInfo>(namespaces.Count);
            Array.ForEach(namespaces.ToArray(), x => _namespaces.Put(x));
        }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(string endpointName, PartitioningIntent partitioningIntent)
        {
            if (partitioningIntent == PartitioningIntent.Sending)
            {
                var @namespace = _namespaces.Get();
                yield return new RuntimeNamespaceInfo(@namespace.Name, @namespace.ConnectionString, NamespaceMode.Active);
            }

            if (partitioningIntent == PartitioningIntent.Receiving || partitioningIntent == PartitioningIntent.Creating)
            {
                var mode = NamespaceMode.Active;
                for (var i = 0; i < _namespaces.Size; i++)
                {
                    var @namespace = _namespaces.Get();
                    yield return new RuntimeNamespaceInfo(@namespace.Name, @namespace.ConnectionString, mode);
                    mode = NamespaceMode.Passive;
                }
            }
        }
    }
}