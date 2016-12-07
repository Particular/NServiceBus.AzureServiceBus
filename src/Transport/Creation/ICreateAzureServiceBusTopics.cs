namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    interface ICreateAzureServiceBusTopicsInternal
    {
        Task<TopicDescription> Create(string topicPath, INamespaceManagerInternal namespaceManager);
    }
}