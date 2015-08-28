namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.Creation
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    public interface ICreateAzureServiceBusTopics
    {
        Task<TopicDescription> CreateAsync(string topicPath, NamespaceManager namespaceManager);
    }
}