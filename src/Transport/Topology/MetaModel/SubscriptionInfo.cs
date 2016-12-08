namespace NServiceBus.Transport.AzureServiceBus
{
    class SubscriptionInfoInternal : EntityInfoInternal
    {
        public IBrokerSideSubscriptionFilterInternal BrokerSideFilter { get; set; }

        public IClientSideSubscriptionFilterInternal ClientSideFilter { get; set; }

        public SubscriptionMetadataInternal Metadata { get; set; }
    }
}