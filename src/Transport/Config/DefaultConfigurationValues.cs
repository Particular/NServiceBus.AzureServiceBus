namespace NServiceBus.AzureServiceBus
{
    using System;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Settings;
    using Topology.MetaModel;
    using Transport.AzureServiceBus;

    class DefaultConfigurationValues
    {
        public SettingsHolder Apply(SettingsHolder settings)
        {
            ApplyDefaultsForConnectivity(settings);
            ApplyDefaultValuesForAddressing(settings);
            ApplyDefaultValuesForQueueDescriptions(settings);
            ApplyDefaultValuesForTopics(settings);
            ApplyDefaultValuesForSubscriptions(settings);
            ApplyDefaultValuesForRules(settings);
            ApplyDefaultValuesForSerialization(settings);
            ApplyDefaultValuesForBrokeredMessageConventions(settings);
            ApplyDefaultValuesForSatelliteTransportAddresses(settings);
            return settings;
        }

        static void ApplyDefaultValuesForSatelliteTransportAddresses(SettingsHolder settings)
        {
            settings.SetDefault<SatelliteTransportAddressCollection>(new SatelliteTransportAddressCollection());
        }

        void ApplyDefaultValuesForBrokeredMessageConventions(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.BrokeredMessageConventions.ToIncomingMessageConverter, typeof(DefaultBrokeredMessagesToIncomingMessagesConverter));
            settings.SetDefault(WellKnownConfigurationKeys.BrokeredMessageConventions.FromOutgoingMessageConverter, typeof(DefaultBatchedOperationsToBrokeredMessagesConverter));
        }

        void ApplyDefaultValuesForSerialization(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Serialization.BrokeredMessageBodyType, SupportedBrokeredMessageBodyTypes.ByteArray);
        }

        void ApplyDefaultValuesForAddressing(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, new NamespaceConfigurations());
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.UseNamespaceAliasesInsteadOfConnectionStrings, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.DefaultNamespaceAlias, "default");
        }

        void ApplyDefaultsForConnectivity(SettingsHolder settings)
        {
            var numberOfLogicalCores = Math.Max(2, Environment.ProcessorCount);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity, numberOfLogicalCores);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue, true);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.ConnectivityMode, ConnectivityMode.Tcp);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.TransportType, TransportType.NetMessaging); // can't make TransportType.Amqp the default due to performance issues investigated with ASB team
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace, numberOfLogicalCores);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode, ReceiveMode.PeekLock);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageReceivers.AutoRenewTimeout, TimeSpan.FromMinutes(5.0));
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount, 20);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessagingFactories.BatchFlushInterval, TimeSpan.FromSeconds(0.5));
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.BackOffTimeOnThrottle, TimeSpan.FromSeconds(10));
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryAttemptsOnThrottle, 5);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximumMessageSizeInKilobytes, 256);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.MessageSizePaddingPercentage, 5);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.OversizedBrokeredMessageHandlerInstance, new ThrowOnOversizedBrokeredMessages());
        }

        void ApplyDefaultValuesForQueueDescriptions(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathMaximumLength, 260);

//            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.LockDuration, TimeSpan.FromMilliseconds(30000));
//            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxSizeInMegabytes, (long) 1024);
//            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresDuplicateDetection, false);
//            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.DefaultMessageTimeToLive, TimeSpan.MaxValue);
//            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableDeadLetteringOnMessageExpiration, false);
//            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.DuplicateDetectionHistoryTimeWindow, TimeSpan.FromMilliseconds(600000));
//
//            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxDeliveryCount, GetNumberOfImmediateRetries(settings));
//
//            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnablePartitioning, false);
//            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.SupportOrdering, false);
//            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.AutoDeleteOnIdle, TimeSpan.MaxValue);
//            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableBatchedOperations, true);
//
//            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpress, false);
//            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpressCondition, new Func<string, bool>(name => true));
//
//            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesToCondition, new Func<string, bool>(name => true));
//            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesTo, null);
        }

        static int GetNumberOfImmediateRetries(SettingsHolder settings)
        {
            var maxDeliveryCount = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Core.RecoverabilityNumberOfImmediateRetries);
            maxDeliveryCount = maxDeliveryCount > 0 ? maxDeliveryCount + 1 : 10;
            return maxDeliveryCount;
        }

        void ApplyDefaultValuesForTopics(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathMaximumLength, 260);

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
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameMaximumLength, 50);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.AutoDeleteOnIdle, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.DefaultMessageTimeToLive, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.LockDuration, TimeSpan.FromMilliseconds(30000));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableBatchedOperations, true);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableDeadLetteringOnFilterEvaluationExceptions, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableDeadLetteringOnMessageExpiration, false);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.MaxDeliveryCount, GetNumberOfImmediateRetries(settings));

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesToCondition, new Func<string, bool>(name => true));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesTo, null);
        }

        static void ApplyDefaultValuesForRules(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameMaximumLength, 50);
        }
    }
}
