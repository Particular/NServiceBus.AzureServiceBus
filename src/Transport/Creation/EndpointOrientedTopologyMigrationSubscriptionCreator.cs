namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    class EndpointOrientedTopologyMigrationSubscriptionCreator : ICreateAzureServiceBusSubscriptionsInternal
    {
        AzureServiceBusSubscriptionCreatorV6 azureServiceBusSubscriptionCreatorV6;
        AzureServiceBusForwardingSubscriptionCreator azureServiceBusForwardingSubscriptionCreator;

        public EndpointOrientedTopologyMigrationSubscriptionCreator(AzureServiceBusForwardingSubscriptionCreator fowardingCreator, AzureServiceBusSubscriptionCreatorV6 v6CompatibleCreator)
        {
            azureServiceBusForwardingSubscriptionCreator = fowardingCreator;
            azureServiceBusSubscriptionCreatorV6 = v6CompatibleCreator;
        }

        public Task<SubscriptionDescription> Create(string topicPath, string subscriptionName, SubscriptionMetadataInternal metadata, string sqlFilter, INamespaceManagerInternal namespaceManager, string forwardTo)
        {
            if (metadata is ForwardingTopologySubscriptionMetadata)
            {
                return azureServiceBusForwardingSubscriptionCreator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardTo);
            }

            return azureServiceBusSubscriptionCreatorV6.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardTo);
        }

        public Task DeleteSubscription(string topicPath, string subscriptionName, SubscriptionMetadataInternal metadata, string sqlFilter, INamespaceManagerInternal namespaceManager, string forwardTo)
        {
            if (metadata is ForwardingTopologySubscriptionMetadata)
            {
                return azureServiceBusForwardingSubscriptionCreator.DeleteSubscription(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardTo);
            }

            return azureServiceBusSubscriptionCreatorV6.DeleteSubscription(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardTo);
        }
    }
}