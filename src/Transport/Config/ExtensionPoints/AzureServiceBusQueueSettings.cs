namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusQueueSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusQueueSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        public AzureServiceBusQueueSettings DescriptionFactory(Func<string, ReadOnlySettings, QueueDescription> factory)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.DescriptionFactory, factory);

            return this;
        }

        public AzureServiceBusQueueSettings ForwardTo(string forwardTo)
        {
            return ForwardTo(n => true, forwardTo);
        }
        public AzureServiceBusQueueSettings ForwardTo(Func<string, bool> condition, string forwardTo)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardToCondition, condition);
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardTo, forwardTo);

            return this;
        }

        public AzureServiceBusQueueSettings ForwardDeadLetteredMessagesTo(string forwardDeadLetteredMessagesTo)
        {
            return ForwardDeadLetteredMessagesTo(n => true, forwardDeadLetteredMessagesTo);
        }
        public AzureServiceBusQueueSettings ForwardDeadLetteredMessagesTo(Func<string, bool> condition, string forwardDeadLetteredMessagesTo)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesToCondition, condition);
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesTo, forwardDeadLetteredMessagesTo);

            return this;
        }

        public AzureServiceBusQueueSettings EnableExpress(bool enableExpress)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpress, enableExpress);

            return this;
        }

        public AzureServiceBusQueueSettings EnableExpress(Func<string, bool> condition, bool enableExpress)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpressCondition, condition);
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpress, enableExpress);

            return this;
        }

        public AzureServiceBusQueueSettings AutoDeleteOnIdle(TimeSpan autoDeleteOnIdle)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.AutoDeleteOnIdle, autoDeleteOnIdle);

            return this;
        }

        public AzureServiceBusQueueSettings EnablePartitioning(bool enablePartitioning)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.EnablePartitioning, enablePartitioning);

            return this;
        }

        public AzureServiceBusQueueSettings EnableBatchedOperations(bool enableBatchedOperations)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableBatchedOperations, enableBatchedOperations);

            return this;
        }

        public AzureServiceBusQueueSettings MaxDeliveryCount(int maxDeliveryCount)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxDeliveryCount, maxDeliveryCount);

            return this;
        }

        public AzureServiceBusQueueSettings DuplicateDetectionHistoryTimeWindow(TimeSpan duplicateDetectionHistoryTimeWindow)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.DuplicateDetectionHistoryTimeWindow, duplicateDetectionHistoryTimeWindow);

            return this;
        }

        public AzureServiceBusQueueSettings EnableDeadLetteringOnMessageExpiration(bool enableDeadLetteringOnMessageExpiration)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableDeadLetteringOnMessageExpiration, enableDeadLetteringOnMessageExpiration);

            return this;
        }

        public AzureServiceBusQueueSettings DefaultMessageTimeToLive(TimeSpan defaultMessageTimeToLive)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.DefaultMessageTimeToLive, defaultMessageTimeToLive);

            return this;
        }

        public AzureServiceBusQueueSettings RequiresSession(bool requiresSession)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresSession, requiresSession);

            return this;
        }

        public AzureServiceBusQueueSettings RequiresDuplicateDetection(bool requiresDuplicateDetection)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresDuplicateDetection, requiresDuplicateDetection);

            return this;
        }


        public AzureServiceBusQueueSettings MaxSizeInMegabytes(long maxSizeInMegabytes)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxSizeInMegabytes, maxSizeInMegabytes);

            return this;
        }

        public AzureServiceBusQueueSettings LockDuration(TimeSpan duration)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.LockDuration, duration);

            return this;
        }

        public AzureServiceBusQueueSettings SupportOrdering(bool supported)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.SupportOrdering, supported);

            return this;
        }
    }
}