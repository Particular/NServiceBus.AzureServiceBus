namespace NServiceBus.Transport.AzureServiceBus
{
    class ForwardingTopologySubscriptionMetadata : SubscriptionMetadataInternal
    {
        public RuntimeNamespaceInfo NamespaceInfo { get; set; }
        public string SubscribedEventFullName { get; set; }
    }
}