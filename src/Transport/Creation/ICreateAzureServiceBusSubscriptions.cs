namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public interface ICreateAzureServiceBusSubscriptions
    {
        Task<SubscriptionDescription> CreateAsync(string topicPath, string subscriptionName, INamespaceManager namespaceManager);
    }
}