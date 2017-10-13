namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;

    /// <summary>
    /// Contract to implement custom namespace partitioning.
    /// </summary>
    public interface INamespacePartitioningStrategy
    {
        /// <summary>
        /// Return a set of namespaces required by strategy for <see cref="PartitioningIntent"/>.
        /// </summary>
        IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent);
    }
}