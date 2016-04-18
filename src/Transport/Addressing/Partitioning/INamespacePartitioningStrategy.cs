namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;

    public interface INamespacePartitioningStrategy
    {
        IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent);
    }
}
