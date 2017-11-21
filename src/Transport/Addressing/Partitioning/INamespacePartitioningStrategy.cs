namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;

    /// <summary>
    /// Contract to implement custom namespace partitioning.
    /// </summary>
    public interface INamespacePartitioningStrategy
    {
        /// <summary>
        /// Gets whether the information returned by the strategy for <see cref="PartitioningIntent.Sending" /> is cacheable.
        /// The flag should be set to <c>false</c> if the partitioning strategy is intended to return a new runtime namespace for
        /// every routing decision.
        /// For static information that never changes the flag can be set to <c>true</c> thus the runtime namespaces will only ever
        /// be acquired once during the lifetime of an endpoint per destination.
        /// </summary>
        bool SendingCacheable { get; }

        /// <summary>
        /// Return a set of namespaces required by strategy for <see cref="PartitioningIntent" />.
        /// </summary>
        IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent);
    }
}