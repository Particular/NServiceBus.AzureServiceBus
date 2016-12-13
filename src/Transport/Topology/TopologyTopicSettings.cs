namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;

    class TopologyTopicSettings
    {
        // TODO: no longer a factory but a Modifier?
        public Action<TopicDescription> DescriptionFactory { get; set; } = description => { };
        public bool SupportOrdering { get; set; } = false;
        public SizeInMegabytes MaxSizeInMegabytes { get; set; } = SizeInMegabytes.Size1024;
        public TimeSpan DefaultMessageTimeToLive { get; set; } = TimeSpan.MaxValue;
        public bool RequiresDuplicateDetection { get; set; } = false;
        public TimeSpan DuplicateDetectionHistoryTimeWindow { get; set; } = TimeSpan.FromMinutes(10);
        public bool EnableBatchedOperations { get; set; } = true;
        public bool EnablePartitioning { get; set; } = false;
        public TimeSpan AutoDeleteOnIdle { get; set; } = TimeSpan.MaxValue;
        public bool EnableExpress { get; set; } = false;
        // TODO: no longer relevant since factory is replace with Modifier
        public Func<string, bool> EnableExpressCondition { get; set; } = name => true;
        public bool EnableFilteringMessagesBeforePublishing { get; set; } = false;
    }
}