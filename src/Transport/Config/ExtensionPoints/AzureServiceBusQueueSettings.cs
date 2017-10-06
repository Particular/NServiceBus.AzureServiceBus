namespace NServiceBus
{
    using System;
    using Configuration.AdvancedExtensibility;
    using Microsoft.ServiceBus.Messaging;
    using Settings;
    using Transport.AzureServiceBus;

    public partial class AzureServiceBusQueueSettings : ExposeSettings
    {
        internal AzureServiceBusQueueSettings(SettingsHolder settings) : base(settings)
        {
            queueSettings = settings.GetOrCreate<TopologySettings>().QueueSettings;
        }

        /// <summary>
        /// Customize queue creation.
        /// </summary>
        public AzureServiceBusQueueSettings DescriptionCustomizer(Action<QueueDescription> customizer)
        {
            queueSettings.DescriptionCustomizer = customizer;

            return this;
        }

        /// <summary>
        /// <remarks> Default is set not to forward.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings ForwardDeadLetteredMessagesTo(string forwardDeadLetteredMessagesTo)
        {
            return ForwardDeadLetteredMessagesTo(n => true, forwardDeadLetteredMessagesTo);
        }

        /// <summary>
        /// <remarks> Default is set not to forward.</remarks>
        /// </summary>
        // TODO: needs to be deprecated
        public AzureServiceBusQueueSettings ForwardDeadLetteredMessagesTo(Func<string, bool> condition, string forwardDeadLetteredMessagesTo)
        {
            queueSettings.ForwardDeadLetteredMessagesTo = forwardDeadLetteredMessagesTo;
            queueSettings.ForwardDeadLetteredMessagesToCondition = condition;

            return this;
        }

        /// <summary>
        /// <remarks> Default is TimeSpan.MaxValue.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings AutoDeleteOnIdle(TimeSpan autoDeleteOnIdle)
        {
            queueSettings.AutoDeleteOnIdle = autoDeleteOnIdle;

            return this;
        }


        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings EnablePartitioning(bool enablePartitioning)
        {
            queueSettings.EnablePartitioning = enablePartitioning;

            return this;
        }

        /// <summary>
        /// <remarks> Default is true.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings EnableBatchedOperations(bool enableBatchedOperations)
        {
            queueSettings.EnableBatchedOperations = enableBatchedOperations;

            return this;
        }

        /// <summary>
        /// <remarks> Default is 10.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings MaxDeliveryCount(int maxDeliveryCount)
        {
            queueSettings.MaxDeliveryCount = maxDeliveryCount;

            return this;
        }

        /// <summary>
        /// <remarks> Default is 10 minutes.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings DuplicateDetectionHistoryTimeWindow(TimeSpan duplicateDetectionHistoryTimeWindow)
        {
            queueSettings.DuplicateDetectionHistoryTimeWindow = duplicateDetectionHistoryTimeWindow;

            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings EnableDeadLetteringOnMessageExpiration(bool enableDeadLetteringOnMessageExpiration)
        {
            queueSettings.EnableDeadLetteringOnMessageExpiration = enableDeadLetteringOnMessageExpiration;

            return this;
        }

        /// <summary>
        /// <remarks> Default is TimeSpan.MaxValue.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings DefaultMessageTimeToLive(TimeSpan defaultMessageTimeToLive)
        {
            queueSettings.DefaultMessageTimeToLive = defaultMessageTimeToLive;

            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings RequiresDuplicateDetection(bool requiresDuplicateDetection)
        {
            queueSettings.RequiresDuplicateDetection = requiresDuplicateDetection;

            return this;
        }


        /// <summary>
        /// <remarks> Default is 1,024 MB.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings MaxSizeInMegabytes(SizeInMegabytes maxSizeInMegabytes)
        {
            queueSettings.MaxSizeInMegabytes = maxSizeInMegabytes;

            return this;
        }

        /// <summary>
        /// <remarks> Default is 30 seconds.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings LockDuration(TimeSpan duration)
        {
            queueSettings.LockDuration = duration;

            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings SupportOrdering(bool supportOrdering)
        {
            queueSettings.SupportOrdering = supportOrdering;

            return this;
        }

        TopologyQueueSettings queueSettings;
    }
}