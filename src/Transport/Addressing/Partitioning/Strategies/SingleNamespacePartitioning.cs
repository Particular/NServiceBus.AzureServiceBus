namespace NServiceBus
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Settings;
    using Transport.AzureServiceBus;

    public class SingleNamespacePartitioning : INamespacePartitioningStrategy, ICacheSendingNamespaces
    {
        NamespaceConfigurations namespaces;

        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public SingleNamespacePartitioning(ReadOnlySettings settings)
        {
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, out namespaces))
            {
                throw new ConfigurationErrorsException($"The '{nameof(SingleNamespacePartitioning)}' strategy requires exactly one namespace, please configure the connection string to your azure servicebus namespace.");
            }

            namespaces = new NamespaceConfigurations(namespaces.Where(n => n.Purpose == NamespacePurpose.Partitioning).ToList());

            if (namespaces.Count != 1)
            {
                throw new ConfigurationErrorsException($"The '{nameof(SingleNamespacePartitioning)}' strategy requires exactly one namespace for the purpose of partitioning, found {namespaces.Count}. Please remove additional namespace registrations.");
            }

            SendingNamespacesCanBeCached = true;
        }

        /// <summary>
        /// Gets whether the information returned by the strategy for <see cref="PartitioningIntent.Sending"/> is cache-able.
        /// </summary>
        public bool SendingNamespacesCanBeCached { get; }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent)
        {
            var @namespace = namespaces.First();
            yield return new RuntimeNamespaceInfo(@namespace.Alias, @namespace.ConnectionString, @namespace.Purpose, NamespaceMode.Active);
        }
    }
}