namespace NServiceBus.AzureServiceBus
{
    public class SubscriptionInfo : EntityInfo
    {
        public IBrokerSideSubscriptionFilter BrokerSideFilter { get; set; }

        public IClientSideSubscriptionFilter ClientSideFilter { get; set; }

        public SubscriptionMetadata Metadata { get; set; }
    }
}