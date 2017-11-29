namespace NServiceBus.Transport.AzureServiceBus
{
    public interface ICacheSendingNamespaces
    {
        /// <summary>
        /// Gets whether the information returned by the strategy for <see cref="PartitioningIntent.Sending" /> is cache-able.
        /// The flag should be set to <c>false</c> if the partitioning strategy is intended to return a different runtime namespace for every routing decision.
        /// For static information that never changes the flag can be set to <c>true</c> thus the runtime namespaces will only ever
        /// be acquired once during the lifetime of an endpoint per destination.
        /// </summary>
        bool SendingNamespacesCanBeCached { get; }
    }
}