namespace NServiceBus.AzureServiceBus
{
    public class SubscriptionInfo : EntityInfo
    {
        public IBrokerSideSubscriptionFilter BrokerSideFilter { get; set; }

        public IClientSideSubscriptionFilter ClientSideFilter { get; set; }

        public SubscriptionMetadata Metadata { get; set; }
    }

    public class SubscriptionMetadata
    {
        public string Description { get; set; }
        public string SubscriptionNameBasedOnEventWithNamespace { get; set; }
    }

    class ForwardingTopologySubscriptionMetadata : SubscriptionMetadata
    {
        public NamespaceInfo NamespaceInfo { get; set; }
    }
}