namespace NServiceBus.Transport.AzureServiceBus

{
    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class SubscriptionMetadata
    {
        public string Description { get; set; }
        public string SubscriptionNameBasedOnEventWithNamespace { get; set; }
    }
}