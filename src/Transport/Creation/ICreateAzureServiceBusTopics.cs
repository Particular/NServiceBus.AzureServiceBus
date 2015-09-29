namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public interface ICreateAzureServiceBusTopics
    {
        Task<TopicDescription> CreateAsync(string topicPath, INamespaceManager namespaceManager);
    }
}