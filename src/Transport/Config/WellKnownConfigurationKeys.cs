﻿// ReSharper disable MemberHidesStaticFromOuterClass
namespace NServiceBus.AzureServiceBus
{
    static class WellKnownConfigurationKeys
    {
        public static class Topology
        {
            public const string Publishers = "AzureServiceBus.Settings.Topology.Publishers";

            public static class Resources
            {
                public static class Queues
                {
                    public const string DescriptionFactory = "AzureServiceBus.Settings.Topology.Resources.Queues.DescriptionFactory";
                    public const string SupportOrdering = "AzureServiceBus.Settings.Topology.Resources.Queues.SupportOrdering";
                    public const string LockDuration = "AzureServiceBus.Settings.Topology.Resources.Queues.LockDuration";
                    public const string MaxSizeInMegabytes = "AzureServiceBus.Settings.Topology.Resources.Queues.MaxSizeInMegabytes";
                    public const string RequiresDuplicateDetection = "AzureServiceBus.Settings.Topology.Resources.Queues.RequiresDuplicateDetection";
                    public const string DefaultMessageTimeToLive = "AzureServiceBus.Settings.Topology.Resources.Queues.DefaultMessageTimeToLive";
                    public const string EnableDeadLetteringOnMessageExpiration = "AzureServiceBus.Settings.Topology.Resources.Queues.EnableDeadLetteringOnMessageExpiration";
                    public const string DuplicateDetectionHistoryTimeWindow = "AzureServiceBus.Settings.Topology.Resources.Queues.DuplicateDetectionHistoryTimeWindow";
                    public const string MaxDeliveryCount = "AzureServiceBus.Settings.Topology.Resources.Queues.MaxDeliveryCount";
                    public const string EnableBatchedOperations = "AzureServiceBus.Settings.Topology.Resources.Queues.EnableBatchedOperations";
                    public const string EnablePartitioning = "AzureServiceBus.Settings.Topology.Resources.Queues.EnablePartitioning";
                    public const string AutoDeleteOnIdle = "AzureServiceBus.Settings.Topology.Resources.Queues.AutoDeleteOnIdle";

                    public const string EnableExpress = "AzureServiceBus.Settings.Topology.Resources.Queues.EnableExpress";
                    public const string EnableExpressCondition = "AzureServiceBus.Settings.Topology.Resources.Queues.EnableExpressCondition";

                    public const string ForwardDeadLetteredMessagesToCondition = "AzureServiceBus.Settings.Topology.Resources.Queues.ForwardDeadLetteredMessagesToCondition";
                    public const string ForwardDeadLetteredMessagesTo = "AzureServiceBus.Settings.Topology.Resources.Queues.ForwardDeadLetteredMessagesTo";
                }

                public static class Topics
                {
                    public const string DescriptionFactory = "AzureServiceBus.Settings.Topology.Resources.Topics.DescriptionFactory";

                    public const string AutoDeleteOnIdle = "AzureServiceBus.Settings.Topology.Resources.Topics.AutoDeleteOnIdle";
                    public const string DefaultMessageTimeToLive = "AzureServiceBus.Settings.Topology.Resources.Topics.DefaultMessageTimeToLive";
                    public const string DuplicateDetectionHistoryTimeWindow = "AzureServiceBus.Settings.Topology.Resources.Topics.DuplicateDetectionHistoryTimeWindow";
                    public const string EnableBatchedOperations = "AzureServiceBus.Settings.Topology.Resources.Topics.EnableBatchedOperations";
                    public const string EnableFilteringMessagesBeforePublishing = "AzureServiceBus.Settings.Topology.Resources.Topics.EnableFilteringMessagesBeforePublishing";
                    public const string EnablePartitioning = "AzureServiceBus.Settings.Topology.Resources.Topics.EnablePartitioning";
                    public const string MaxSizeInMegabytes = "AzureServiceBus.Settings.Topology.Resources.Topics.MaxSizeInMegabytes";
                    public const string RequiresDuplicateDetection = "AzureServiceBus.Settings.Topology.Resources.Topics.RequiresDuplicateDetection";
                    public const string SupportOrdering = "AzureServiceBus.Settings.Topology.Resources.Topics.SupportOrdering";

                    public const string EnableExpress = "AzureServiceBus.Settings.Topology.Resources.Topics.EnableExpress";
                    public const string EnableExpressCondition = "AzureServiceBus.Settings.Topology.Resources.Topics.EnableExpressCondition";
                }

                public static class Subscriptions
                {
                    public const string DescriptionFactory = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.DescriptionFactory";

                    public const string AutoDeleteOnIdle = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.AutoDeleteOnIdle";
                    public const string DefaultMessageTimeToLive = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.DefaultMessageTimeToLive";
                    public const string EnableBatchedOperations = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.EnableBatchedOperations";
                    public const string EnableDeadLetteringOnFilterEvaluationExceptions = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.EnableDeadLetteringOnFilterEvaluationExceptions";
                    public const string EnableDeadLetteringOnMessageExpiration = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.EnableDeadLetteringOnMessageExpiration";
                    public const string LockDuration = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.LockDuration";
                    public const string MaxDeliveryCount = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.MaxDeliveryCount";

                    public const string ForwardDeadLetteredMessagesTo = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesTo";
                    public const string ForwardDeadLetteredMessagesToCondition = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesToCondition";
                }


            }

            public static class Addressing
            {
                public const string UseNamespaceNamesInsteadOfConnectionStrings = "AzureServiceBus.Settings.Topology.Addressing.UseNamespaceNamesInsteadOfConnectionStrings";
                public const string DefaultNamespaceName = "AzureServiceBus.Settings.Topology.Addressing.DefaultNamespaceName";
                public const string Namespaces = "AzureServiceBus.Settings.Topology.Addressing.Namespaces";

                public static class Partitioning
                {
                    public const string Strategy = "AzureServiceBus.Settings.Topology.Addressing.Partitioning.Strategy";
                }

                public static class Composition
                {
                    public const string Strategy = "AzureServiceBus.Settings.Topology.Addressing.Composition.Strategy";
                    public const string HierarchyCompositionPathGenerator = "AzureServiceBus.Settings.Topology.Addressing.Composition.HierarchyCompositionPathGenerator";
                }

                public static class Sanitization
                {
                    public const string Strategy = "AzureServiceBus.Settings.Topology.Addressing.Sanitization.Strategy";
                    public const string QueuePathMaximumLength = "AzureServiceBus.Settings.Topology.Addressing.Sanitization.QueuePathMaximumLength";
                    public const string TopicPathMaximumLength = "AzureServiceBus.Settings.Topology.Addressing.Sanitization.TopicPathMaximumLength";
                    public const string SubscriptionNameMaximumLength = "AzureServiceBus.Settings.Topology.Addressing.Sanitization.SubscriptionNameMaximumLength";
                    public const string RuleNameMaximumLength = "AzureServiceBus.Settings.Topology.Addressing.Sanitization.RuleNameMaximumLength";
                    public const string QueuePathValidator = "AzureServiceBus.Settings.Topology.Addressing.Sanitization.QueuePathValidator";
                    public const string TopicPathValidator = "AzureServiceBus.Settings.Topology.Addressing.Sanitization.TopicPathValidator";
                    public const string SubscriptionNameValidator = "AzureServiceBus.Settings.Topology.Addressing.Sanitization.SubscriptionNameValidator";
                    public const string RuleNameValidator = "AzureServiceBus.Settings.Topology.Addressing.Sanitization.RuleNameValidator";
                    public const string QueuePathSanitizer = "AzureServiceBus.Settings.Topology.Addressing.Sanitization.QueuePathSanitizer";
                    public const string TopicPathSanitizer = "AzureServiceBus.Settings.Topology.Addressing.Sanitization.TopicPathSanitizer";
                    public const string SubscriptionNameSanitizer = "AzureServiceBus.Settings.Topology.Addressing.Sanitization.SubscriptionNameSanitizer";
                    public const string RuleNameSanitizer = "AzureServiceBus.Settings.Topology.Addressing.Sanitization.RuleNameSanitizer";
                    public const string Hash = "AzureServiceBus.Settings.Topology.Addressing.Sanitization.Hash";
                }

                public static class Individualization
                {
                    public const string Strategy = "AzureServiceBus.Settings.Topology.Addressing.Individualization.Strategy";
                    public const string DiscriminatorBasedIndividualizationDiscriminatorGenerator = "AzureServiceBus.Settings.Topology.Addressing.Individualization.DiscriminatorBasedIndividualizationDiscriminatorGenerator";
                }
            }

            public static class Bundling
            {
                public const string NumberOfEntitiesInBundle = "AzureServiceBus.Settings.Topology.Bundling.NumberOfEntitiesInBundle";
                public const string BundlePrefix = "AzureServiceBus.Settings.Topology.Bundling.BundlePrefix";
            }

        }

        public static class Connectivity
        {
            public const string NumberOfClientsPerEntity = "AzureServiceBus.Connectivity.NumberOfClientsPerEntity";
            public const string SendViaReceiveQueue = "AzureServiceBus.Connectivity.SendViaReceiveQueue";
            public const string ConnectivityMode = "AzureServiceBus.Connectivity.ConnectivityMode";

            public static class MessageReceivers
            {
                public const string ReceiveMode = "AzureServiceBus.Connectivity.MessageReceivers.ReceiveMode";
                public const string PrefetchCount = "AzureServiceBus.Connectivity.MessageReceivers.PrefetchCount";
                public const string RetryPolicy = "AzureServiceBus.Connectivity.MessageReceivers.RetryPolicy";
                public const string AutoRenewTimeout = "AzureServiceBus.Connectivity.MessageReceivers.AutoRenewTimeout";
            }

            public static class MessageSenders
            {
                public const string RetryPolicy = "AzureServiceBus.Connectivity.MessageSenders.RetryPolicy";
                public const string BackOffTimeOnThrottle = "AzureServiceBus.Connectivity.MessageSenders.BackOffTimeOnThrottle";
                public const string RetryAttemptsOnThrottle = "AzureServiceBus.Connectivity.MessageSenders.RetryAttemptsOnThrottle";
                public const string MaximumMessageSizeInKilobytes = "AzureServiceBus.Connectivity.MessageSenders.MaximumMessageSizeInKilobytes";
                public const string MessageSizePaddingPercentage = "AzureServiceBus.Connectivity.MessageSenders.MessageSizePaddingPercentage";
                public const string OversizedBrokeredMessageHandlerInstance = "AzureServiceBus.Connectivity.MessageSenders.OversizedBrokeredMessageHandlerInstance";
            }

            public static class MessagingFactories
            {
                public const string RetryPolicy = "AzureServiceBus.Connectivity.MessagingFactories.RetryPolicy";
                public const string NumberOfMessagingFactoriesPerNamespace = "AzureServiceBus.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace";
                public const string MessagingFactorySettingsFactory = "AzureServiceBus.Connectivity.MessagingFactories.MessagingFactorySettingsFactory";
                public const string BatchFlushInterval = "AzureServiceBus.Connectivity.MessagingFactories.BatchFlushInterval";
            }

            public static class NamespaceManagers
            {
                public const string RetryPolicy = "AzureServiceBus.Connectivity.NamespaceManagers.RetryPolicy";
                public const string NamespaceManagerSettingsFactory = "AzureServiceBus.Connectivity.NamespaceManagers.NamespaceManagerSettingsFactory";
                public const string TokenProviderFactory = "AzureServiceBus.Connectivity.NamespaceManagers.TokenProviderFactory";
            }
        }

        internal static class Core
        {
            public const string CreateTopology = "Transport.CreateQueues";
        }

        public static class Serialization
        {
            public const string BrokeredMessageBodyType = "AzureServiceBus.Serialization.BrokeredMessageBodyType";

        }

        public static class BrokeredMessageConventions
        {
            public const string ToIncomingMessageConverter = "AzureServiceBus.BrokeredMessageConventions.ToIncomingMessageConverter";
            public const string FromOutgoingMessageConverter = "AzureServiceBus.BrokeredMessageConventions.FromOutgoingMessageConverter";
        }
    }
}