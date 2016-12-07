namespace NServiceBus.Transport.AzureServiceBus
{
    class SubscriptionInfoInternal : EntityInfoInternal
    {
        public IBrokerSideSubscriptionFilter BrokerSideFilter { get; set; }

        public IClientSideSubscriptionFilter ClientSideFilter { get; set; }

        public SubscriptionMetadataInternal Metadata { get; set; }
    }
}