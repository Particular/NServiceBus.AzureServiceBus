namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;

    public interface INamespacePartitioningStrategy
    {
        IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent);
    }
}
