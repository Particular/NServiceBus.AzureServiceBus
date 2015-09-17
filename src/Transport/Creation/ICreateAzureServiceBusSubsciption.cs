namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.Creation
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus;

    public interface ICreateAzureServiceBusSubsciption
    {
        Task<SubscriptionDescription> CreateAsync(string topicPath, string subscriptionName, INamespaceManager namespaceManager);
    }
}