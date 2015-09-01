namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.Creation
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus;

    public interface ICreateAzureServiceBusTopics
    {
        Task<TopicDescription> CreateAsync(string topicPath, INamespaceManager namespaceManager);
    }
}