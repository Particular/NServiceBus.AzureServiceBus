namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Settings;

    public class DefaultConfigurationValues
    {
        public SettingsHolder Apply(SettingsHolder settings)
        {
            ApplyDefaultsForConnectivity(settings);
            ApplyDefaultValuesForAddressing(settings);
            ApplyDefaultValuesForQueueDescriptions(settings);
            ApplyDefaultValuesForTopics(settings);
            ApplyDefaultValuesForSubscriptions(settings);
            ApplyDefaultValuesForSerialization(settings);
            ApplyDefaultValuesForValidation(settings);

            return settings;
        }

        void ApplyDefaultValuesForValidation(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Validation.QueuePathMaximumLength, 260);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Validation.TopicPathMaximumLength, 260);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Validation.SubscriptionPathMaximumLength, 50);
        }

        void ApplyDefaultValuesForSerialization(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Serialization.BrokeredMessageBodyType, SupportedBrokeredMessageBodyTypes.ByteArray);
        }

        void ApplyDefaultValuesForAddressing(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.UseLogicalNamespaceName, (Func<NamespaceInfo, string>)(x => x.ConnectionString));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, new List<string>());
        }

        void ApplyDefaultsForConnectivity(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity, 5);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue, false);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace, 5);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode, ReceiveMode.PeekLock);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageReceivers.AutoRenewTimeout, TimeSpan.FromMinutes(5.0));
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessagingFactories.BatchFlushInterval, TimeSpan.FromSeconds(0.5));
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.BackOffTimeOnThrottle, TimeSpan.FromSeconds(10));
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryAttemptsOnThrottle, 5);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximumMessageSizeInKilobytes, 256);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.MessageSizePaddingPercentage, 5);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.OversizedBrokeredMessageHandlerInstance, new ThrowOnOversizedBrokeredMessages());
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

            var maxDeliveryCount = 6;// (!settings.HasExplicitValue(typeof(FirstLevelRetries).FullName) || settings.IsFeatureEnabled(typeof(FirstLevelRetries))) ? 6 : 1;
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxDeliveryCount, maxDeliveryCount);
            
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresSession, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnablePartitioning, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.SupportOrdering, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.AutoDeleteOnIdle, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableBatchedOperations, true);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpress, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpressCondition, new Func<string, bool>(name => true));

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesToCondition, new Func<string, bool>(name => true));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesTo, null);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardToCondition, new Func<string, bool>( name => true) );
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardTo, null);
        }

        void ApplyDefaultValuesForTopics(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.AutoDeleteOnIdle, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.DefaultMessageTimeToLive, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.DuplicateDetectionHistoryTimeWindow, TimeSpan.FromMilliseconds(600000));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableBatchedOperations, true);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableExpress, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableExpressCondition, new Func<string, bool>(name => true));
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

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesToCondition, new Func<string, bool>(name => true));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesTo, null);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardToCondition, new Func<string, bool>(name => true));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardTo, null);
        }
    }
}