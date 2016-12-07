namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    interface ICreateAzureServiceBusQueuesInternal
    {
        Task<QueueDescription> Create(string queuePath, INamespaceManagerInternal namespaceManager);
    }
}