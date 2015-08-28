namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public interface ICreateAzureServiceBusQueues
    {
        Task<QueueDescription> CreateAsync(string queuePath, INamespaceManager namespaceManager);
    }
}