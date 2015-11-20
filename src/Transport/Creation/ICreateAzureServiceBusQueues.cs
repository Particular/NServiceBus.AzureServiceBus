namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public interface ICreateAzureServiceBusQueues
    {
        Task<QueueDescription> Create(string queuePath, INamespaceManager namespaceManager);
    }
}