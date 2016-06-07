namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Settings;

    public class RoundRobinNamespacePartitioning : INamespacePartitioningStrategy
    {
        CircularBuffer<NamespaceInfo> namespaces;

        public RoundRobinNamespacePartitioning(ReadOnlySettings settings)
        {
            NamespaceConfigurations namespaces;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out namespaces) || namespaces.Count <= 1)
            {
                throw new ConfigurationErrorsException("The 'RoundRobin' namespace partitioning strategy requires more than one namespace, please configure multiple azure servicebus namespaces");
            }

            this.namespaces = new CircularBuffer<NamespaceInfo>(namespaces.Count);
            Array.ForEach(namespaces.ToArray(), x => this.namespaces.Put(x));
        }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent)
        {
            if (partitioningIntent == PartitioningIntent.Sending)
            {
                var @namespace = namespaces.Get();
                yield return new RuntimeNamespaceInfo(@namespace.Name, @namespace.ConnectionString, @namespace.Purpose, NamespaceMode.Active);
            }

            if (partitioningIntent == PartitioningIntent.Receiving || partitioningIntent == PartitioningIntent.Creating)
            {
                var mode = NamespaceMode.Active;
                for (var i = 0; i < namespaces.Size; i++)
                {
                    var @namespace = namespaces.Get();
                    yield return new RuntimeNamespaceInfo(@namespace.Name, @namespace.ConnectionString, @namespace.Purpose, mode);
                    mode = NamespaceMode.Passive;
                }
            }
        }
    }
}