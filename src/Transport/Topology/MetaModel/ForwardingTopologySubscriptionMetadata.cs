namespace NServiceBus.AzureServiceBus
{
    class ForwardingTopologySubscriptionMetadata : SubscriptionMetadata
    {
        public RuntimeNamespaceInfo NamespaceInfo { get; set; }
        public string SubscribedEventFullName { get; set; }
    }
}