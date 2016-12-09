namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;

    class TopologySettings
    {
        public TopologyQueueSettings QueueSettings { get; set; } = new TopologyQueueSettings();
    }

    class TopologyQueueSettings
    {
        public Action<QueueDescription> DescriptionFactory { get; set; } = description => { };
        public bool SupportOrdering { get; set; } = false;
        public TimeSpan LockDuration { get; set; } = TimeSpan.FromSeconds(30);
        public SizeInMegabytes MaxSizeInMegabytes { get; set; } = SizeInMegabytes.Size1024;
        public bool RequiresDuplicateDetection { get; set; } = false;
        public TimeSpan DefaultMessageTimeToLive { get; set; } = TimeSpan.MaxValue;
        public bool EnableDeadLetteringOnMessageExpiration { get; set; } = false;
        public TimeSpan DuplicateDetectionHistoryTimeWindow { get; set; } = TimeSpan.FromMinutes(10);
        public int MaxDeliveryCount { get; set; } = 10;
        public bool EnableBatchedOperations { get; set; } = true;
        public bool EnablePartitioning { get; set; } = false;
        public TimeSpan AutoDeleteOnIdle { get; set; } = TimeSpan.MaxValue;
        public bool EnableExpress { get; set; } = false;
        public Func<string, bool> EnableExpressCondition { get; set; } = name => true;
        public string ForwardDeadLetteredMessagesTo { get; set; } = null;
        public Func<string, bool> ForwardDeadLetteredMessagesToCondition { get; set; } = name => true;
    }
}