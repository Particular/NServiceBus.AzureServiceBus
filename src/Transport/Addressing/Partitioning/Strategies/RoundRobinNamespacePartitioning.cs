namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Settings;
    using Transport.AzureServiceBus;

    public class RoundRobinNamespacePartitioning : INamespacePartitioningStrategy
    {
        public void Initialize(ReadOnlySettings settings)
        {
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, out NamespaceConfigurations namespaces))
            {
                throw new ConfigurationErrorsException($"The '{nameof(RoundRobinNamespacePartitioning)}' strategy requires more than one namespace, please use {nameof(AzureServiceBusTransportExtensions.NamespacePartitioning)}().{nameof(AzureServiceBusNamespacePartitioningSettings.AddNamespace)}() to register multiple namespaces");
            }

            namespaces = new NamespaceConfigurations(namespaces.Where(n => n.Purpose == NamespacePurpose.Partitioning).ToList());

            if (namespaces.Count <= 1)
            {
                throw new ConfigurationErrorsException($"The '{nameof(RoundRobinNamespacePartitioning)}' strategy requires more than one namespace for the purpose of partitioning, found {namespaces.Count}. , please use {nameof(AzureServiceBusTransportExtensions.NamespacePartitioning)}().{nameof(AzureServiceBusNamespacePartitioningSettings.AddNamespace)}() to register additional namespaces");
            }

            namespacesBuffer = new CircularBuffer<NamespaceInfo>(namespaces.Count);
            Array.ForEach(namespaces.ToArray(), x => this.namespacesBuffer.Put(x));
        }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent)
        {
            if (partitioningIntent == PartitioningIntent.Sending)
            {
                var @namespace = namespacesBuffer.Get();
                yield return new RuntimeNamespaceInfo(@namespace.Alias, @namespace.Connection, @namespace.Purpose, NamespaceMode.Active);
            }

            if (partitioningIntent == PartitioningIntent.Receiving || partitioningIntent == PartitioningIntent.Creating)
            {
                var mode = NamespaceMode.Active;
                for (var i = 0; i < namespacesBuffer.Size; i++)
                {
                    var @namespace = namespacesBuffer.Get();
                    yield return new RuntimeNamespaceInfo(@namespace.Alias, @namespace.Connection, @namespace.Purpose, mode);
                    mode = NamespaceMode.Passive;
                }
            }
        }

        CircularBuffer<NamespaceInfo> namespacesBuffer;
    }
}