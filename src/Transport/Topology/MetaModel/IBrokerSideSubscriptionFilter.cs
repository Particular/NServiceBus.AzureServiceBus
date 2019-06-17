namespace NServiceBus.Transport.AzureServiceBus
{
    /// <summary>
    /// Provides native format filter which can be injected into the broker (subscription case)
    /// </summary>
    public interface IBrokerSideSubscriptionFilter
    {
        /// <summary>
        /// serialized the filter into native format, so that it can be injected into the broker (subscription case)
        /// </summary>
        string Serialize();
    }
}