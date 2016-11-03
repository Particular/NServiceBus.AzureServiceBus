namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;

    public interface IPartitioningStrategy
    {
        void Initialize(NamespaceInfo[] namespacesForPartitioning);

        IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent);
    }
}