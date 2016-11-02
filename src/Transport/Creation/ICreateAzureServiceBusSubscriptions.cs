namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateAzureServiceBusSubscriptions
    {
        Task<SubscriptionDescription> Create(string topicPath, string subscriptionName, SubscriptionMetadata metadata, string sqlFilter, INamespaceManager namespaceManager, string forwardTo);
    }

    // TODO Move into internalized ICreateAzureServiceBusSubscriptions in v8
    interface ICreateAzureServiceBusSubscriptionsAbleToDeleteSubscriptions
    {
        Task DeleteSubscription(string topicPath, string subscriptionName, SubscriptionMetadata metadata, string sqlFilter, INamespaceManager namespaceManager, string forwardTo);
    }
}