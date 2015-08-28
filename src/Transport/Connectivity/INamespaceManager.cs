namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    public interface INamespaceManager
    {
        NamespaceManagerSettings Settings { get; }
        Uri Address { get; }

        Task CreateQueueAsync(QueueDescription description);
        Task UpdateQueueAsync(QueueDescription description);
        Task DeleteQueueAsync(string path);

        Task<QueueDescription> GetQueueAsync(string path);
        Task<bool> QueueExistsAsync(string path);
    }
}