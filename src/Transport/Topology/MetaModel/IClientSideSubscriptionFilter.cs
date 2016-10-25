namespace NServiceBus.Transport.AzureServiceBus
{
    [ObsoleteEx(Message = "Internal unutilized contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IClientSideSubscriptionFilter
    {
        /// <summary>
        /// executes a filter in memory, if it is impossible to inject it into the broker (eventhub case)
        /// </summary>
        bool Execute(object message);
    }
}