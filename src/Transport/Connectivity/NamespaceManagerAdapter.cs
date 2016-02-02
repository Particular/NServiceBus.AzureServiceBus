namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
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

        public bool HasManageRights
        {
            get
            {
                try
                {
                    _manager.GetQueues();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public Task CreateQueue(QueueDescription description)
        {
            return _manager.CreateQueueAsync(description);
        }

        public Task UpdateQueue(QueueDescription description)
        {
            return _manager.UpdateQueueAsync(description);
        }

        public Task<QueueDescription> GetQueue(string path)
        {
            return _manager.GetQueueAsync(path);
        }

        public Task<bool> QueueExists(string path)
        {
            return _manager.QueueExistsAsync(path);
        }

        public async Task<TopicDescription> CreateTopic(TopicDescription topicDescription)
        {
            return await _manager.CreateTopicAsync(topicDescription).ConfigureAwait(false);
        }

        public async Task DeleteQueue(string path)
        {
            await _manager.DeleteQueueAsync(path).ConfigureAwait(false);
        }

        public Task DeleteTopic(string path)
        {
            return _manager.DeleteTopicAsync(path);
        }

        public Task<bool> SubscriptionExists(string topicPath, string subscriptionName)
        {
            return _manager.SubscriptionExistsAsync(topicPath, subscriptionName);
        }

        public Task<SubscriptionDescription> CreateSubscription(SubscriptionDescription subscriptionDescription, string sqlFilter)
        {
            return _manager.CreateSubscriptionAsync(subscriptionDescription, new SqlFilter(sqlFilter));
        }

        public Task<SubscriptionDescription> GetSubscription(string topicPath, string subscriptionName)
        {
            return _manager.GetSubscriptionAsync(topicPath, subscriptionName);
        }

        public Task<SubscriptionDescription> UpdateSubscription(SubscriptionDescription subscriptionDescription)
        {
            return _manager.UpdateSubscriptionAsync(subscriptionDescription);
        }

        public Task<IEnumerable<RuleDescription>> GetRules(SubscriptionDescription subscriptionDescription)
        {
            return _manager.GetRulesAsync(subscriptionDescription.TopicPath, subscriptionDescription.Name);
        }

        public Task<TopicDescription> UpdateTopic(TopicDescription topicDescription)
        {
            return _manager.UpdateTopicAsync(topicDescription);
        }

        public Task<bool> TopicExists(string path)
        {
            return _manager.TopicExistsAsync(path);
        }

        public Task<TopicDescription> GetTopic(string path)
        {
            return _manager.GetTopicAsync(path);
        }

        public Task DeleteSubscriptionAsync(string topicPath, string subscriptionName)
        {
            return _manager.DeleteSubscriptionAsync(topicPath, subscriptionName);
        }
    }
}