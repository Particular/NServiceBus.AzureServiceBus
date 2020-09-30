namespace NServiceBus.Transport.AzureServiceBus
{
    /// <summary>
    /// Partitioning intent for a namespace.
    /// </summary>
    public enum PartitioningIntent
    {
        /// <summary>Namespace should be used to receive from.</summary>
        Receiving,

        /// <summary>Namespace should be used to send to.</summary>
        Sending,

        /// <summary>Namespace should be used to create entities.</summary>
        Creating
    }
}