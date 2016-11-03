namespace NServiceBus
{
    using System.Collections.Generic;
    using System.Configuration;
    using Transport.AzureServiceBus;

    public class RoundRobinPartitioning : IPartitioningStrategy
    {
        CircularBuffer<NamespaceInfo> namespaces;

        public void Initialize(NamespaceInfo[] namespacesForPartitioning)
        {
            if (namespacesForPartitioning.Length == 0)
            {
                throw new ConfigurationErrorsException($"The '{nameof(RoundRobinNamespacePartitioning)}' strategy requires more than one namespace, please use {nameof(AzureServiceBusTransportExtensions.NamespacePartitioning)}().{nameof(AzureServiceBusNamespacePartitioningSettings.AddNamespace)}() to register multiple namespaces");
            }

            if (namespacesForPartitioning.Length <= 1)
            {
                throw new ConfigurationErrorsException($"The '{nameof(RoundRobinNamespacePartitioning)}' strategy requires more than one namespace for the purpose of partitioning, found {namespacesForPartitioning.Length}. , please use {nameof(AzureServiceBusTransportExtensions.NamespacePartitioning)}().{nameof(AzureServiceBusNamespacePartitioningSettings.AddNamespace)}() to register additional namespaces");
            }

            namespaces = new CircularBuffer<NamespaceInfo>(namespacesForPartitioning.Length);
            namespaces.Put(namespacesForPartitioning);
        }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent)
        {
            if (partitioningIntent == PartitioningIntent.Sending)
            {
                var @namespace = namespaces.Get();
                yield return new RuntimeNamespaceInfo(@namespace.Alias, @namespace.ConnectionString, @namespace.Purpose, NamespaceMode.Active);
            }

            if (partitioningIntent == PartitioningIntent.Receiving || partitioningIntent == PartitioningIntent.Creating)
            {
                var mode = NamespaceMode.Active;
                for (var i = 0; i < namespaces.Size; i++)
                {
                    var @namespace = namespaces.Get();
                    yield return new RuntimeNamespaceInfo(@namespace.Alias, @namespace.ConnectionString, @namespace.Purpose, mode);
                    mode = NamespaceMode.Passive;
                }
            }
        }
    }
}