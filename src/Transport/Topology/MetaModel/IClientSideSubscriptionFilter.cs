namespace NServiceBus.Transport.AzureServiceBus
{
    interface IClientSideSubscriptionFilterInternal
    {
        /// <summary>
        /// executes a filter in memory, if it is impossible to inject it into the broker (eventhub case)
        /// </summary>
        bool Execute(object message);
    }
}