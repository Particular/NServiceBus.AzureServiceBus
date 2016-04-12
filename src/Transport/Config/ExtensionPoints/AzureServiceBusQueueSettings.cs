namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using AzureServiceBus;
    using Configuration.AdvanceExtensibility;
    using Settings;

    public class AzureServiceBusQueueSettings : ExposeSettings
    {
        SettingsHolder settings;

        public AzureServiceBusQueueSettings(SettingsHolder settings)
            : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Customize queue creation by providing <see cref="QueueDescription"/>.
        /// </summary>
        public AzureServiceBusQueueSettings DescriptionFactory(Func<string, ReadOnlySettings, QueueDescription> factory)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.DescriptionFactory, factory);

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
        public AzureServiceBusQueueSettings ForwardDeadLetteredMessagesTo(Func<string, bool> condition, string forwardDeadLetteredMessagesTo)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesToCondition, condition);
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesTo, forwardDeadLetteredMessagesTo);

            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings EnableExpress(bool enableExpress)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpress, enableExpress);

            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings EnableExpress(Func<string, bool> condition, bool enableExpress)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpressCondition, condition);
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpress, enableExpress);

            return this;
        }

        /// <summary>
        /// <remarks> Default is TimeSpan.MaxValue.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings AutoDeleteOnIdle(TimeSpan autoDeleteOnIdle)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.AutoDeleteOnIdle, autoDeleteOnIdle);

            return this;
        }


        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings EnablePartitioning(bool enablePartitioning)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.EnablePartitioning, enablePartitioning);

            return this;
        }

        /// <summary>
        /// <remarks> Default is true.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings EnableBatchedOperations(bool enableBatchedOperations)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableBatchedOperations, enableBatchedOperations);

            return this;
        }

        /// <summary>
        /// <remarks> Default is 10.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings MaxDeliveryCount(int maxDeliveryCount)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxDeliveryCount, maxDeliveryCount);

            return this;
        }

        /// <summary>
        /// <remarks> Default is 10 minutes.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings DuplicateDetectionHistoryTimeWindow(TimeSpan duplicateDetectionHistoryTimeWindow)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.DuplicateDetectionHistoryTimeWindow, duplicateDetectionHistoryTimeWindow);

            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings EnableDeadLetteringOnMessageExpiration(bool enableDeadLetteringOnMessageExpiration)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableDeadLetteringOnMessageExpiration, enableDeadLetteringOnMessageExpiration);

            return this;
        }

        /// <summary>
        /// <remarks> Default is TimeSpan.MaxValue.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings DefaultMessageTimeToLive(TimeSpan defaultMessageTimeToLive)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.DefaultMessageTimeToLive, defaultMessageTimeToLive);

            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings RequiresSession(bool requiresSession)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresSession, requiresSession);

            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings RequiresDuplicateDetection(bool requiresDuplicateDetection)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresDuplicateDetection, requiresDuplicateDetection);

            return this;
        }


        /// <summary>
        /// <remarks> Default is 1,024 MB.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings MaxSizeInMegabytes(SizeInMegabytes maxSizeInMegabytes)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxSizeInMegabytes, (long)maxSizeInMegabytes);

            return this;
        }

        /// <summary>
        /// <remarks> Default is 30 seconds.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings LockDuration(TimeSpan duration)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.LockDuration, duration);

            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusQueueSettings SupportOrdering(bool supported)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.SupportOrdering, supported);

            return this;
        }
    }
}