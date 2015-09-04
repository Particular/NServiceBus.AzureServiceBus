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

        Task<TopicDescription> CreateTopicAsync(TopicDescription topicDescription);
        Task<TopicDescription> GetTopicAsync(string path);
        Task<TopicDescription> UpdateTopicAsync(TopicDescription topicDescription);
        Task<bool> TopicExistsAsync(string path);
        Task DeleteTopicAsync(string path);

        Task<bool> SubscriptionExistsAsync(string topicPath, string subscriptionName);
        Task<SubscriptionDescription> CreateSubscriptionAsync(SubscriptionDescription subscriptionDescription);
        Task<SubscriptionDescription> GetSubscriptionAsync(string topicPath, string subscriptionName);
        Task<SubscriptionDescription> UpdateSubscriptionAsync(SubscriptionDescription subscriptionDescription);
    }
}