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

        public async Task<TopicDescription> CreateTopicAsync(TopicDescription topicDescription)
        {
            return await _manager.CreateTopicAsync(topicDescription);
        }

        public async Task DeleteQueueAsync(string path)
        {
            await _manager.DeleteQueueAsync(path);
        }

        public async Task DeleteTopicAsync(string path)
        {
            await _manager.DeleteTopicAsync(path);
        }

        public Task<bool> SubscriptionExistsAsync(string topicPath, string subscriptionName)
        {
            return _manager.SubscriptionExistsAsync(topicPath, subscriptionName);
        }

        public async Task<SubscriptionDescription> CreateSubscriptionAsync(SubscriptionDescription subscriptionDescription)
        {
            return await _manager.CreateSubscriptionAsync(subscriptionDescription);
        }

        public async Task<SubscriptionDescription> GetSubscriptionAsync(string topicPath, string subscriptionName)
        {
            return await _manager.GetSubscriptionAsync(topicPath, subscriptionName);
        }

        public async Task<SubscriptionDescription> UpdateSubscriptionAsync(SubscriptionDescription subscriptionDescription)
        {
            return await _manager.UpdateSubscriptionAsync(subscriptionDescription);
        }

        public async Task<TopicDescription> UpdateTopicAsync(TopicDescription topicDescription)
        {
            return await _manager.UpdateTopicAsync(topicDescription);
        }

        public async Task<bool> TopicExistsAsync(string path)
        {
            return await _manager.TopicExistsAsync(path);
        }

        public async Task<TopicDescription> GetTopicAsync(string path)
        {
            return await _manager.GetTopicAsync(path);
        }

        public async Task DeleteSubsciptionAsync(string topicPath, string subscriptionName)
        {
            await _manager.DeleteSubscriptionAsync(topicPath, subscriptionName);
        }
    }
}