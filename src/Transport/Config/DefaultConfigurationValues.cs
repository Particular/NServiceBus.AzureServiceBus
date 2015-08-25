﻿namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Settings;

    public class DefaultConfigurationValues
    {
        public SettingsHolder Apply(SettingsHolder settings)
        {
            ApplyDefaultsForConnectivity(settings);
            ApplyDefaultValuesForQueueDescriptions(settings);
            ApplyDefaultValuesForTopics(settings);
            ApplyDefaultValuesForSubscriptions(settings);

            return settings;
        }

        void ApplyDefaultsForConnectivity(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity, 5);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace, 5);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode, ReceiveMode.PeekLock);
        }

        void ApplyDefaultValuesForQueueDescriptions(SettingsHolder settings)
        {
            settings.Set("Transport.CreateQueues", true);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.LockDuration, TimeSpan.FromMilliseconds(30000));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxSizeInMegabytes, (long) 1024);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresDuplicateDetection, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresSession, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.DefaultMessageTimeToLive, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableDeadLetteringOnMessageExpiration, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.DuplicateDetectionHistoryTimeWindow, TimeSpan.FromMilliseconds(600000));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxDeliveryCount, 6);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresSession, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnablePartitioning, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.SupportOrdering, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.AutoDeleteOnIdle, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpress, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableBatchedOperations, true);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesToCondition, new Func<string, bool>(name => true));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesTo, null);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardToCondition, new Func<string, bool>( name => true) );
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardTo, null);
        }

        void ApplyDefaultValuesForTopics(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.AutoDeleteOnIdle, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.DefaultMessageTimeToLive, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.DuplicateDetectionHistoryTimeWindow, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.SupportOrdering, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.DuplicateDetectionHistoryTimeWindow, TimeSpan.FromMilliseconds(600000));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableBatchedOperations, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableExpress, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableFilteringMessagesBeforePublishing, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.EnablePartitioning, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.MaxSizeInMegabytes, (long)1024);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.RequiresDuplicateDetection, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.SupportOrdering, false);
        }

        void ApplyDefaultValuesForSubscriptions(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.AutoDeleteOnIdle, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.DefaultMessageTimeToLive, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.LockDuration, TimeSpan.FromMilliseconds(30000));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableBatchedOperations, true);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableDeadLetteringOnFilterEvaluationExceptions, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableDeadLetteringOnMessageExpiration, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.RequiresSession, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.MaxDeliveryCount, 6);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.SupportOrdering, false);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesToCondition, new Func<string, bool>(name => true));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesTo, null);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardToCondition, new Func<string, bool>(name => true));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardTo, null);
        }
    }
}