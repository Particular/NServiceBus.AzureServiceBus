namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    public class NamespaceManagerAdapter : INamespaceManager
    {
        readonly NamespaceManager _manager;

        public NamespaceManagerAdapter(NamespaceManager manager)
        {
            _manager = manager;
        }

        public NamespaceManagerSettings Settings => _manager.Settings;

        public Uri Address => _manager.Address;

        public Task CreateQueueAsync(QueueDescription description)
        {
            return _manager.CreateQueueAsync(description);
        }

        public Task UpdateQueueAsync(QueueDescription description)
        {
            return _manager.UpdateQueueAsync(description);
        }

        public Task<QueueDescription> GetQueueAsync(string path)
        {
            return _manager.GetQueueAsync(path);
        }

        public Task<bool> QueueExistsAsync(string path)
        {
            return _manager.QueueExistsAsync(path);
        }

        public async Task<TopicDescription> CreateTopicAsync(TopicDescription topicDescription)
        {
            return await _manager.CreateTopicAsync(topicDescription).ConfigureAwait(false);
        }

        public async Task DeleteQueueAsync(string path)
        {
            await _manager.DeleteQueueAsync(path).ConfigureAwait(false);
        }

        public Task DeleteTopicAsync(string path)
        {
            return _manager.DeleteTopicAsync(path);
        }

        public Task<bool> SubscriptionExistsAsync(string topicPath, string subscriptionName)
        {
            return _manager.SubscriptionExistsAsync(topicPath, subscriptionName);
        }

        public Task<SubscriptionDescription> CreateSubscriptionAsync(SubscriptionDescription subscriptionDescription)
        {
            return _manager.CreateSubscriptionAsync(subscriptionDescription);
        }

        public Task<SubscriptionDescription> GetSubscriptionAsync(string topicPath, string subscriptionName)
        {
            return _manager.GetSubscriptionAsync(topicPath, subscriptionName);
        }

        public Task<SubscriptionDescription> UpdateSubscriptionAsync(SubscriptionDescription subscriptionDescription)
        {
            return _manager.UpdateSubscriptionAsync(subscriptionDescription);
        }

        public Task<TopicDescription> UpdateTopicAsync(TopicDescription topicDescription)
        {
            return _manager.UpdateTopicAsync(topicDescription);
        }

        public Task<bool> TopicExistsAsync(string path)
        {
            return _manager.TopicExistsAsync(path);
        }

        public Task<TopicDescription> GetTopicAsync(string path)
        {
            return _manager.GetTopicAsync(path);
        }

        public Task DeleteSubscriptionAsync(string topicPath, string subscriptionName)
        {
            return _manager.DeleteSubscriptionAsync(topicPath, subscriptionName);
        }
    }
}