namespace NServiceBus.Transport.AzureServiceBus
{
    class TopologySettings
    {
        public TopologyQueueSettings QueueSettings { get; set; } = new TopologyQueueSettings();
        public TopologyTopicSettings TopicSettings { get; set; } = new TopologyTopicSettings();
        public TopologySubscriptionSettings SubscriptionSettings { get; set; } = new TopologySubscriptionSettings();
    }
}