namespace NServiceBus.Transport.AzureServiceBus
{
    class TopologySettings
    {
        public TopologyQueueSettings QueueSettings { get; } = new TopologyQueueSettings();
        public TopologyTopicSettings TopicSettings { get; } = new TopologyTopicSettings();
        public TopologySubscriptionSettings SubscriptionSettings { get; } = new TopologySubscriptionSettings();
    }
}