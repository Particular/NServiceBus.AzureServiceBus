namespace NServiceBus.Transport.AzureServiceBus
{
    public interface IClientSideSubscriptionFilter
    {
        /// <summary>
        /// executes a filter in memory, if it is impossible to inject it into the broker (eventhub case)
        /// </summary>
        bool Execute(object message);
    }
}