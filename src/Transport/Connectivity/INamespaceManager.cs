namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    public interface INamespaceManager
    {
        NamespaceManagerSettings Settings { get; }
        Uri Address { get; }

        Task CreateQueue(QueueDescription description);
        Task UpdateQueue(QueueDescription description);
        Task DeleteQueue(string path);
        Task<QueueDescription> GetQueue(string path);
        Task<bool> QueueExists(string path);

        Task<TopicDescription> CreateTopic(TopicDescription topicDescription);
        Task<TopicDescription> GetTopic(string path);
        Task<TopicDescription> UpdateTopic(TopicDescription topicDescription);
        Task<bool> TopicExists(string path);
        Task DeleteTopic(string path);

        Task<bool> SubscriptionExists(string topicPath, string subscriptionName);
        Task<SubscriptionDescription> CreateSubscription(SubscriptionDescription subscriptionDescription, string sqlFilter);
        Task<SubscriptionDescription> GetSubscription(string topicPath, string subscriptionName);
        Task<SubscriptionDescription> UpdateSubscription(SubscriptionDescription subscriptionDescription);

        Task<IEnumerable<RuleDescription>> GetRules(SubscriptionDescription subscriptionDescription);
    }
}