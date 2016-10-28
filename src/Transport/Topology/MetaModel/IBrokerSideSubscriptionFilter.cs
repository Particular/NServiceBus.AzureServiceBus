namespace NServiceBus.Transport.AzureServiceBus
{
    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IBrokerSideSubscriptionFilter
    {
        /// <summary>
        /// serialized the filter into native format, so that it can be injected into the broker (subscription case)
        /// </summary>
        string Serialize();
    }
}