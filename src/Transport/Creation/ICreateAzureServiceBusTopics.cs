namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public interface ICreateAzureServiceBusTopics
    {
        Task<TopicDescription> Create(string topicPath, INamespaceManager namespaceManager);
    }
}