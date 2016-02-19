namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NServiceBus.Settings;

    public class RoundRobinNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        private readonly CircularBuffer<NamespaceDefinition> _namespaces;

        public RoundRobinNamespacePartitioningStrategy(ReadOnlySettings settings)
        {
            NamespacesDefinition namespaces;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out namespaces) || namespaces.Count <= 1)
            {
                throw new ConfigurationErrorsException("The 'RoundRobin' namespace partitioning strategy requires more than one namespace, please configure multiple azure servicebus namespaces");
            }

            _namespaces = new CircularBuffer<NamespaceDefinition>(namespaces.Count);
            Array.ForEach(namespaces.ToArray(), x => _namespaces.Put(x));
        }

        public IEnumerable<NamespaceInfo> GetNamespaces(string endpointName, PartitioningIntent partitioningIntent)
        {
            if (partitioningIntent == PartitioningIntent.Sending)
            {
                var definition = _namespaces.Get();
                yield return new NamespaceInfo(definition.ConnectionString, NamespaceMode.Active);
            }

            if (partitioningIntent == PartitioningIntent.Receiving || partitioningIntent == PartitioningIntent.Creating)
            {
                var mode = NamespaceMode.Active;
                for (var i = 0; i < _namespaces.Size; i++)
                {
                    var definition = _namespaces.Get();
                    yield return new NamespaceInfo(definition.ConnectionString, mode);
                    mode = NamespaceMode.Passive;
                }
            }
        }
    }
}