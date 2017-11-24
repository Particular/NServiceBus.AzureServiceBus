namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Addressing
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Settings;
    using Transport.AzureServiceBus;

    public class RoundRobinPartitioningFailoverStrategy : INamespacePartitioningStrategy
    {
        public RoundRobinPartitioningFailoverStrategy(ReadOnlySettings settings)
        {
            NamespaceConfigurations namespaces;
            if (!settings.TryGet("AzureServiceBus.Settings.Topology.Addressing.Namespaces", out namespaces))
            {
                throw new ConfigurationErrorsException($"The '{nameof(RoundRobinPartitioningFailoverStrategy)}' strategy requires exactly two namespaces to be configured, please use {nameof(AzureServiceBusTransportExtensions.NamespacePartitioning)}().{nameof(AzureServiceBusNamespacePartitioningSettings.AddNamespace)}() to register the namespaces.");
            }
            var partitioningNamespaces = namespaces.Where(x => x.Purpose == NamespacePurpose.Partitioning).ToList();
            if (partitioningNamespaces.Count != 2)
            {
                throw new ConfigurationErrorsException($"The '{nameof(RoundRobinPartitioningFailoverStrategy)}' strategy requires exactly two namespaces to be configured, please use {nameof(AzureServiceBusTransportExtensions.NamespacePartitioning)}().{nameof(AzureServiceBusNamespacePartitioningSettings.AddNamespace)}() to register the namespaces.");
            }
            this.namespaces = new CircularBuffer<RuntimeNamespaceInfo[]>(partitioningNamespaces.Count);
            var first = namespaces.First();
            var second = namespaces.Last();

            this.namespaces.Put(new[]
            {
                new RuntimeNamespaceInfo(first.Alias, first.ConnectionString, first.Purpose, NamespaceMode.Active),
                new RuntimeNamespaceInfo(second.Alias, second.ConnectionString, second.Purpose, NamespaceMode.Passive)
            });
            this.namespaces.Put(new[]
            {
                new RuntimeNamespaceInfo(first.Alias, first.ConnectionString, first.Purpose, NamespaceMode.Passive),
                new RuntimeNamespaceInfo(second.Alias, second.ConnectionString, second.Purpose, NamespaceMode.Active)
            });
        }

        public bool SendingNamespacesCanBeCached { get; } = false;

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent)
        {
            return namespaces.Get();
        }

        readonly CircularBuffer<RuntimeNamespaceInfo[]> namespaces;
    }
}