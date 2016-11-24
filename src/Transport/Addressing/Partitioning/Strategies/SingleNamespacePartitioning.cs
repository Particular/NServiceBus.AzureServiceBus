namespace NServiceBus
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using AzureServiceBus.Addressing.Partitioning;
    using Settings;
    using Transport.AzureServiceBus;

    public class SingleNamespacePartitioning : INamespacePartitioningStrategy
    {
        List<RuntimeNamespaceInfo> namespaces;

        public SingleNamespacePartitioning(ReadOnlySettings settings)
        {
            namespaces = settings.GetPartitioningNamespaces().ToList();

            if (namespaces.Count != 1)
            {
                throw new ConfigurationErrorsException($"The '{nameof(SingleNamespacePartitioning)}' strategy requires exactly one namespace for the purpose of partitioning, found {namespaces.Count}. Please remove additional namespace registrations.");
            }
        }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent)
        {
            var @namespace = namespaces.First();
            yield return @namespace;
        }
    }
}