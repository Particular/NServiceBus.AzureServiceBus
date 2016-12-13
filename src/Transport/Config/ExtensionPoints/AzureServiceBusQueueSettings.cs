namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusQueueSettings : ExposeSettings
    {
        TopologyQueueSettings queueSettings;

        internal AzureServiceBusQueueSettings(SettingsHolder settings) : base(settings)
        {
            queueSettings = settings.Get<ITopologyInternal>().Settings.QueueSettings;
        }

        // TODO: needs to be obsoleted with guidance
        public AzureServiceBusQueueSettings DescriptionFactory(Func<string, string, ReadOnlySettings, QueueDescription> factory)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Customize queue creation by providing <see cref="QueueDescription"/>.
        /// </summary>
        public AzureServiceBusQueueSettings DescriptionFactory(Action<QueueDescription> factory)
        {
            queueSettings.DescriptionFactory = factory;

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
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings EnableExpress(bool enableExpress)
        {
            queueSettings.EnableExpress = enableExpress;

            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        // TODO: needs to be deprecated
        public AzureServiceBusQueueSettings EnableExpress(Func<string, bool> condition, bool enableExpress)
        {
            queueSettings.EnableExpress = enableExpress;
            queueSettings.EnableExpressCondition = condition;

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
    }
}