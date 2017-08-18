namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using Settings;

    public interface INamespacePartitioningStrategy
    {
        void Initialize(ReadOnlySettings settings);

        IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent);
    }
}