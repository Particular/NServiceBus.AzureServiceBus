namespace NServiceBus.Transport.AzureServiceBus
{
    public class SubscriptionInfo : EntityInfo
    {
        public IBrokerSideSubscriptionFilter BrokerSideFilter { get; set; }

        [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public IClientSideSubscriptionFilter ClientSideFilter { get; set; }

        public SubscriptionMetadata Metadata { get; set; }
    }
}