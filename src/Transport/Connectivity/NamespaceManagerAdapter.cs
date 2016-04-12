namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    public class NamespaceManagerAdapter : INamespaceManager
    {
        readonly NamespaceManager manager;

        public NamespaceManagerAdapter(NamespaceManager manager)
        {
            this.manager = manager;
        }

        public NamespaceManagerSettings Settings => manager.Settings;

        public Uri Address => manager.Address;

        public async Task<bool> CanManageEntities()
        {
            try
            {
                await manager.GetQueuesAsync().ConfigureAwait(false);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public Task CreateQueue(QueueDescription description)
        {
            return manager.CreateQueueAsync(description);
        }

        public Task UpdateQueue(QueueDescription description)
        {
            return manager.UpdateQueueAsync(description);
        }

        public Task<QueueDescription> GetQueue(string path)
        {
            return manager.GetQueueAsync(path);
        }

        public Task<bool> QueueExists(string path)
        {
            return manager.QueueExistsAsync(path);
        }

        public async Task<TopicDescription> CreateTopic(TopicDescription topicDescription)
        {
            return await manager.CreateTopicAsync(topicDescription).ConfigureAwait(false);
        }

        public async Task DeleteQueue(string path)
        {
            await manager.DeleteQueueAsync(path).ConfigureAwait(false);
        }

        public Task DeleteTopic(string path)
        {
            return manager.DeleteTopicAsync(path);
        }

        public Task<bool> SubscriptionExists(string topicPath, string subscriptionName)
        {
            return manager.SubscriptionExistsAsync(topicPath, subscriptionName);
        }

        public Task<SubscriptionDescription> CreateSubscription(SubscriptionDescription subscriptionDescription, string sqlFilter)
        {
            return manager.CreateSubscriptionAsync(subscriptionDescription, new SqlFilter(sqlFilter));
        }

        public Task<SubscriptionDescription> GetSubscription(string topicPath, string subscriptionName)
        {
            return manager.GetSubscriptionAsync(topicPath, subscriptionName);
        }

        public Task<SubscriptionDescription> UpdateSubscription(SubscriptionDescription subscriptionDescription)
        {
            return manager.UpdateSubscriptionAsync(subscriptionDescription);
        }

        public Task<IEnumerable<RuleDescription>> GetRules(SubscriptionDescription subscriptionDescription)
        {
            return manager.GetRulesAsync(subscriptionDescription.TopicPath, subscriptionDescription.Name);
        }

        public Task<SubscriptionDescription> CreateSubscription(SubscriptionDescription subscriptionDescription, RuleDescription ruleDescription)
        {
            return manager.CreateSubscriptionAsync(subscriptionDescription, ruleDescription);
        }

        public Task<TopicDescription> UpdateTopic(TopicDescription topicDescription)
        {
            return manager.UpdateTopicAsync(topicDescription);
        }

        public Task<bool> TopicExists(string path)
        {
            return manager.TopicExistsAsync(path);
        }

        public Task<TopicDescription> GetTopic(string path)
        {
            return manager.GetTopicAsync(path);
        }

        public Task DeleteSubscriptionAsync(string topicPath, string subscriptionName)
        {
            return manager.DeleteSubscriptionAsync(topicPath, subscriptionName);
        }
    }
}