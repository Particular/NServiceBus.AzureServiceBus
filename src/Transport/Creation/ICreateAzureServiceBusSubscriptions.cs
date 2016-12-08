namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    interface ICreateAzureServiceBusSubscriptionsInternal
    {
        Task<SubscriptionDescription> Create(string topicPath, string subscriptionName, SubscriptionMetadataInternal metadata, string sqlFilter, INamespaceManagerInternal namespaceManager, string forwardTo);

        Task DeleteSubscription(string topicPath, string subscriptionName, SubscriptionMetadataInternal metadata, string sqlFilter, INamespaceManagerInternal namespaceManager, string forwardTo);
    }
}