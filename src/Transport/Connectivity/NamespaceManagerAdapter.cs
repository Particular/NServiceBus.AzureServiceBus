namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    class NamespaceManagerAdapterInternal : INamespaceManagerInternal
    {
        public NamespaceManagerAdapterInternal(NamespaceManager manager)
        {
            this.manager = manager;
        }

        public NamespaceManagerSettings Settings => manager.Settings;

        public Uri Address => manager.Address;

        public async Task<bool> CanManageEntities()
        {
            try
            {
                await manager.GetQueuesAsync("startswith(path, '$#_') eq true").ConfigureAwait(false);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public Task CreateQueue(QueueDescription description) => manager.CreateQueueAsync(description);

        public Task UpdateQueue(QueueDescription description) => manager.UpdateQueueAsync(description);

        public Task<QueueDescription> GetQueue(string path) => manager.GetQueueAsync(path);

        public Task<bool> QueueExists(string path) => manager.QueueExistsAsync(path);

        public Task<TopicDescription> CreateTopic(TopicDescription topicDescription) => manager.CreateTopicAsync(topicDescription);

        public Task DeleteQueue(string path) => manager.DeleteQueueAsync(path);

        public Task DeleteTopic(string path) => manager.DeleteTopicAsync(path);

        public Task<bool> SubscriptionExists(string topicPath, string subscriptionName) => manager.SubscriptionExistsAsync(topicPath, subscriptionName);

        public Task<SubscriptionDescription> CreateSubscription(SubscriptionDescription subscriptionDescription, string sqlFilter) => manager.CreateSubscriptionAsync(subscriptionDescription, new SqlFilter(sqlFilter));

        public Task<SubscriptionDescription> GetSubscription(string topicPath, string subscriptionName) => manager.GetSubscriptionAsync(topicPath, subscriptionName);

        public Task<SubscriptionDescription> UpdateSubscription(SubscriptionDescription subscriptionDescription) => manager.UpdateSubscriptionAsync(subscriptionDescription);

        public Task<IEnumerable<RuleDescription>> GetRules(SubscriptionDescription subscriptionDescription) => manager.GetRulesAsync(subscriptionDescription.TopicPath, subscriptionDescription.Name);

        public Task<SubscriptionDescription> CreateSubscription(SubscriptionDescription subscriptionDescription, RuleDescription ruleDescription) => manager.CreateSubscriptionAsync(subscriptionDescription, ruleDescription);

        public Task<IEnumerable<TopicDescription>> GetTopics(string filter) => manager.GetTopicsAsync(filter);

        public Task<TopicDescription> UpdateTopic(TopicDescription topicDescription) => manager.UpdateTopicAsync(topicDescription);

        public Task<bool> TopicExists(string path) => manager.TopicExistsAsync(path);

        public Task<TopicDescription> GetTopic(string path) => manager.GetTopicAsync(path);

        public Task DeleteSubscription(SubscriptionDescription subscriptionDescription) => manager.DeleteSubscriptionAsync(subscriptionDescription.TopicPath, subscriptionDescription.Name);

        // TODO: delete this once NamespaceManagerAdapterInternal is internalized
        public Task DeleteSubscriptionAsync(string topicPath, string subscriptionName) => DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));

        NamespaceManager manager;
    }
}