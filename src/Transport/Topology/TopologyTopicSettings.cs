namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;

    class TopologyTopicSettings
    {
        public Action<TopicDescription> DescriptionCustomizer { get; set; } = description => { };
        public bool SupportOrdering { get; set; } = false;
        public SizeInMegabytes MaxSizeInMegabytes { get; set; } = SizeInMegabytes.Size1024;
        public TimeSpan DefaultMessageTimeToLive { get; set; } = TimeSpan.MaxValue;
        public bool RequiresDuplicateDetection { get; set; } = false;
        public TimeSpan DuplicateDetectionHistoryTimeWindow { get; set; } = TimeSpan.FromMinutes(10);
        public bool EnableBatchedOperations { get; set; } = true;
        public bool EnablePartitioning { get; set; } = false;
        public TimeSpan AutoDeleteOnIdle { get; set; } = TimeSpan.MaxValue;
        public bool EnableFilteringMessagesBeforePublishing { get; set; } = false;
    }
}