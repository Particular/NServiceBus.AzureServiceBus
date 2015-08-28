namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    class NamespaceManagerAdapter : INamespaceManager
    {
        readonly NamespaceManager _manager;

        public NamespaceManagerAdapter(NamespaceManager manager)
        {
            _manager = manager;
        }

        public NamespaceManagerSettings Settings
        {
            get { return _manager.Settings; }
        }

        public Uri Address
        {
            get { return _manager.Address; }
        }

        public async Task CreateQueueAsync(QueueDescription description)
        {
            await _manager.CreateQueueAsync(description);
        }

        public async Task UpdateQueueAsync(QueueDescription description)
        {
            await _manager.UpdateQueueAsync(description);
        }

        public async Task<QueueDescription> GetQueueAsync(string path)
        {
            return await _manager.GetQueueAsync(path);
        }

        public async Task<bool> QueueExistsAsync(string path)
        {
            return await _manager.QueueExistsAsync(path);
        }

        public async Task DeleteQueueAsync(string path)
        {
            await _manager.DeleteQueueAsync(path);
        }
    }
}