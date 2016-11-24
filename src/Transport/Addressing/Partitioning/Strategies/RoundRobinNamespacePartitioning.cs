namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using AzureServiceBus.Addressing.Partitioning;
    using Settings;
    using Transport.AzureServiceBus;

    public class RoundRobinNamespacePartitioning : INamespacePartitioningStrategy
    {
        CircularBuffer<RuntimeNamespaceInfo> namespaces;

        public RoundRobinNamespacePartitioning(ReadOnlySettings settings)
        {
            var namespaces = settings.GetPartitioningNamespaces();

            if (namespaces.Count <= 1)
            {
                throw new ConfigurationErrorsException($"The '{nameof(RoundRobinNamespacePartitioning)}' strategy requires more than one namespace for the purpose of partitioning, found {namespaces.Count}. , please use {nameof(AzureServiceBusTransportExtensions.NamespacePartitioning)}().{nameof(AzureServiceBusNamespacePartitioningSettings.AddNamespace)}() to register additional namespaces");
            }

            this.namespaces = new CircularBuffer<RuntimeNamespaceInfo>(namespaces.Count);
            Array.ForEach(namespaces.ToArray(), x => this.namespaces.Put(x));
        }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent)
        {
            if (partitioningIntent == PartitioningIntent.Sending)
            {
                var @namespace = namespaces.Get();
                yield return @namespace;
            }

            if (partitioningIntent == PartitioningIntent.Receiving || partitioningIntent == PartitioningIntent.Creating)
            {
                var mode = NamespaceMode.Active;
                for (var i = 0; i < namespaces.Size; i++)
                {
                    var @namespace = namespaces.Get();
                    yield return new RuntimeNamespaceInfo(@namespace.Alias, @namespace.ConnectionString, NamespacePurpose.Partitioning, mode);
                    mode = NamespaceMode.Passive;
                }
            }
        }
    }
}