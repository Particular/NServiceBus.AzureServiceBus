namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;

    class TopologySubscriptionSettings
    {
        public Action<SubscriptionDescription> DescriptionCustomizer { get; set; } = description => { };
        public TimeSpan AutoDeleteOnIdle { get; set; } = TimeSpan.MaxValue;
        public TimeSpan DefaultMessageTimeToLive { get; set; } = TimeSpan.MaxValue;
        public bool EnableBatchedOperations { get; set; } = true;
        public bool EnableDeadLetteringOnFilterEvaluationExceptions { get; set; } = false;
        public bool EnableDeadLetteringOnMessageExpiration { get; set; } = false;
        public TimeSpan LockDuration { get; set; } = TimeSpan.FromSeconds(30);
        public int MaxDeliveryCount { get; set; } = 10;
        public string ForwardDeadLetteredMessagesTo { get; set; } = null;
        // TODO: no longer relevant since factory is replace with Customizer
        public Func<string, bool> ForwardDeadLetteredMessagesToCondition { get; set; } = name => true;
    }
}