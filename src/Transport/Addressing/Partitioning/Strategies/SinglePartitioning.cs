namespace NServiceBus
{
    using System.Collections.Generic;
    using System.Configuration;
    using Transport.AzureServiceBus;

    public class SinglePartitioning : IPartitioningStrategy
    {
        public void Initialize(NamespaceInfo[] namespacesForPartitioning)
        {
            if (namespacesForPartitioning.Length == 0)
            {
                throw new ConfigurationErrorsException($"The '{nameof(SinglePartitioning)}' strategy requires exactly one namespace, please configure the connection string to your azure servicebus namespace.");
            }

            if (namespacesForPartitioning.Length != 1)
            {
                throw new ConfigurationErrorsException($"The '{nameof(SinglePartitioning)}' strategy requires exactly one namespace for the purpose of partitioning, found {namespacesForPartitioning.Length}. Please remove additional namespace registrations.");
            }

            var @namespace = namespacesForPartitioning[0];
            selectedNamespace = new RuntimeNamespaceInfo(@namespace.Alias, @namespace.ConnectionString, @namespace.Purpose, NamespaceMode.Active);
        }

        public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent)
        {
            yield return selectedNamespace;
        }

        RuntimeNamespaceInfo selectedNamespace;
    }
}