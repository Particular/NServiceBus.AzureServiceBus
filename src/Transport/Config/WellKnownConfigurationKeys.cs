// ReSharper disable MemberHidesStaticFromOuterClass
namespace NServiceBus.AzureServiceBus
{
    static class WellKnownConfigurationKeys
    {
        public static class Topology
        {
            public static class Resources
            {
                public static class Queues
                {
                    public const string DescriptionFactory = "AzureServiceBus.Settings.Topology.Resources.Queues.DescriptionFactory";
                    public const string SupportOrdering = "AzureServiceBus.Settings.Topology.Resources.Queues.SupportOrdering";
                    public const string LockDuration = "AzureServiceBus.Settings.Topology.Resources.Queues.LockDuration";
                    public const string MaxSizeInMegabytes = "AzureServiceBus.Settings.Topology.Resources.Queues.MaxSizeInMegabytes";
                    public const string RequiresDuplicateDetection = "AzureServiceBus.Settings.Topology.Resources.Queues.RequiresDuplicateDetection";
                    public const string RequiresSession = "AzureServiceBus.Settings.Topology.Resources.Queues.RequiresSession";
                    public const string DefaultMessageTimeToLive = "AzureServiceBus.Settings.Topology.Resources.Queues.DefaultMessageTimeToLive";
                    public const string EnableDeadLetteringOnMessageExpiration = "AzureServiceBus.Settings.Topology.Resources.Queues.EnableDeadLetteringOnMessageExpiration";
                    public const string DuplicateDetectionHistoryTimeWindow = "AzureServiceBus.Settings.Topology.Resources.Queues.DuplicateDetectionHistoryTimeWindow";
                    public const string MaxDeliveryCount = "AzureServiceBus.Settings.Topology.Resources.Queues.MaxDeliveryCount";
                    public const string EnableBatchedOperations = "AzureServiceBus.Settings.Topology.Resources.Queues.EnableBatchedOperations";
                    public const string EnablePartitioning = "AzureServiceBus.Settings.Topology.Resources.Queues.EnablePartitioning";
                    public const string AutoDeleteOnIdle = "AzureServiceBus.Settings.Topology.Resources.Queues.AutoDeleteOnIdle";
                    public const string EnableExpress = "AzureServiceBus.Settings.Topology.Resources.Queues.EnableExpress";

                    public const string ForwardDeadLetteredMessagesToCondition = "AzureServiceBus.Settings.Topology.Resources.Queues.ForwardDeadLetteredMessagesToCondition";
                    public const string ForwardDeadLetteredMessagesTo = "AzureServiceBus.Settings.Topology.Resources.Queues.ForwardDeadLetteredMessagesTo";

                    public const string ForwardToCondition = "AzureServiceBus.Settings.Topology.Resources.Queues.ForwardToCondition";
                    public const string ForwardTo = "AzureServiceBus.Settings.Topology.Resources.Queues.ForwardTo";
                }

                public static class Topics
                {
                    public const string DescriptionFactory = "AzureServiceBus.Settings.Topology.Resources.Topics.DescriptionFactory";

                    public const string AutoDeleteOnIdle = "AzureServiceBus.Settings.Topology.Resources.Topics.AutoDeleteOnIdle";
                    public const string DefaultMessageTimeToLive = "AzureServiceBus.Settings.Topology.Resources.Topics.DefaultMessageTimeToLive";
                    public const string DuplicateDetectionHistoryTimeWindow = "AzureServiceBus.Settings.Topology.Resources.Topics.DuplicateDetectionHistoryTimeWindow";
                    public const string EnableBatchedOperations = "AzureServiceBus.Settings.Topology.Resources.Topics.EnableBatchedOperations";
                    public const string EnableExpress = "AzureServiceBus.Settings.Topology.Resources.Topics.EnableExpress";
                    public const string EnableFilteringMessagesBeforePublishing = "AzureServiceBus.Settings.Topology.Resources.Topics.EnableFilteringMessagesBeforePublishing";
                    public const string EnablePartitioning = "AzureServiceBus.Settings.Topology.Resources.Topics.EnablePartitioning";
                    public const string MaxSizeInMegabytes = "AzureServiceBus.Settings.Topology.Resources.Topics.MaxSizeInMegabytes";
                    public const string RequiresDuplicateDetection = "AzureServiceBus.Settings.Topology.Resources.Topics.RequiresDuplicateDetection";
                    public const string SupportOrdering = "AzureServiceBus.Settings.Topology.Resources.Topics.SupportOrdering";
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
                    public const string RequiresSession = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.RequiresSession";

                    public const string ForwardDeadLetteredMessagesTo = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesTo";
                    public const string ForwardDeadLetteredMessagesToCondition = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesToCondition";

                    public const string ForwardTo = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.ForwardTo";
                    public const string ForwardToCondition = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.ForwardToCondition";
                }

                
            }

            public static class Addressing
            {
                public static class Partitioning
                {
                    public const string Strategy = "AzureServiceBus.Settings.Topology.Addressing.Partitioning.Strategy";
                    public const string Namespaces = "AzureServiceBus.Settings.Topology.Addressing.Partitioning.Namespaces";
                }

                public static class Composition
                {
                    public const string Strategy = "AzureServiceBus.Settings.Topology.Addressing.Composition.Strategy";
                }

                public static class Validation
                {
                    public const string Strategy = "AzureServiceBus.Settings.Topology.Addressing.Validation.Strategy";
                }

                public static class Sanitization
                {
                    public const string Strategy = "AzureServiceBus.Settings.Topology.Addressing.Sanitization.Strategy";
                }

                public static class Individualization
                {
                    public const string Strategy = "AzureServiceBus.Settings.Topology.Addressing.Individualization.Strategy";
                }
            }
        }

        public static class Connectivity
        {
            public const string NumberOfClientsPerEntity = "AzureServiceBus.Connectivity.NumberOfClientsPerEntity";
            

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
            }

            public static class MessagingFactories
            {
                public const string PrefetchCount = "AzureServiceBus.Connectivity.MessagingFactories.PrefetchCount";
                public const string RetryPolicy = "AzureServiceBus.Connectivity.MessagingFactories.RetryPolicy";
                public const string NumberOfMessagingFactoriesPerNamespace = "AzureServiceBus.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace";
                public const string MessagingFactorySettingsFactory = "AzureServiceBus.Connectivity.MessagingFactories.MessagingFactorySettingsFactory";
                public const string BatchFlushInterval = "AzureServiceBus.Connectivity.MessagingFactories.BatchFlushInterval";
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
    }
}