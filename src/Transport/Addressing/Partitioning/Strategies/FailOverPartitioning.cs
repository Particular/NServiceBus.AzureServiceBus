namespace NServiceBus
{
    using System.Collections.Generic;
    using System.Configuration;
    using Transport.AzureServiceBus;

    public class FailOverPartitioning : IPartitioningStrategy
    {
        public FailOverMode Mode { get; set; }

        public void Initialize(NamespaceInfo[] namespacesForPartitioning)
        {
            if (namespacesForPartitioning.Length == 0)
            {
                throw new ConfigurationErrorsException($"The '{nameof(FailOverPartitioning)}' strategy requires exactly two namespaces to be configured, please use {nameof(AzureServiceBusTransportExtensions.NamespacePartitioning)}().{nameof(AzureServiceBusNamespacePartitioningSettings.AddNamespace)}() to register the namespaces.");
            }

            if (namespacesForPartitioning.Length < 2)
            {
                throw new ConfigurationErrorsException($"The '{nameof(FailOverPartitioning)}' strategy requires exactly two namespaces for the purpose of partitioning, found {namespacesForPartitioning.Length}, please use {nameof(AzureServiceBusTransportExtensions.NamespacePartitioning)}().{nameof(AzureServiceBusNamespacePartitioningSettings.AddNamespace)}() to register additional namespaces.");
            }
            if (namespacesForPartitioning.Length > 2)
            {
                throw new ConfigurationErrorsException($"The '{nameof(FailOverPartitioning)}' strategy requires exactly two namespaces for the purpose of partitioning, found {namespacesForPartitioning.Length}, please register less namespaces.");
            }

            var primary = namespacesForPartitioning[0];
            var secondary = namespacesForPartitioning[1];
            activePrimary = new RuntimeNamespaceInfo(primary.Alias, primary.ConnectionString, primary.Purpose, NamespaceMode.Active);
            activeSecondary = new RuntimeNamespaceInfo(secondary.Alias, secondary.ConnectionString, secondary.Purpose, NamespaceMode.Active);

            passivePrimary = new RuntimeNamespaceInfo(primary.Alias, primary.ConnectionString, primary.Purpose, NamespaceMode.Passive);
            passiveSecondary = new RuntimeNamespaceInfo(secondary.Alias, secondary.ConnectionString, secondary.Purpose, NamespaceMode.Passive);
        }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent)
        {
            if (partitioningIntent == PartitioningIntent.Sending)
            {
                yield return Mode == FailOverMode.Primary
                    ? activePrimary
                    : activeSecondary;
            }

            if (partitioningIntent == PartitioningIntent.Creating || partitioningIntent == PartitioningIntent.Receiving)
            {
                if (Mode == FailOverMode.Primary)
                {
                    yield return activePrimary;
                    yield return activeSecondary;
                }

                if (Mode == FailOverMode.Secondary)
                {
                    yield return passivePrimary;
                    yield return passiveSecondary;
                }
            }
        }

        RuntimeNamespaceInfo activePrimary;
        RuntimeNamespaceInfo activeSecondary;
        RuntimeNamespaceInfo passivePrimary;
        RuntimeNamespaceInfo passiveSecondary;
    }
}