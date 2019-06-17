// ReSharper disable MemberHidesStaticFromOuterClass

namespace NServiceBus.Transport.AzureServiceBus
{
    static class WellKnownConfigurationKeys
    {
        public static class Topology
        {
            public const string Publishers = "AzureServiceBus.Settings.Topology.Publishers";
            public const string Selected = "AzureServiceBus.Settings.Topology.Selected";
            public const string ForwardingTopology = "ForwardingTopology";
            public const string EndpointOrientedTopology = "EndpointOrientedTopology";
            public const string EndpointOrientedMigrationTopology = "EndpointOrientedMigrationTopology";

            public static class Addressing
            {
                public const string UseNamespaceAliasesInsteadOfConnectionStrings = "AzureServiceBus.Settings.Topology.Addressing.UseNamespaceAliasesInsteadOfConnectionStrings";
                public const string DefaultNamespaceAlias = "AzureServiceBus.Settings.Topology.Addressing.DefaultNamespaceAlias";
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
                    public const string BrokerSideSubscriptionFilterFactoryInstance = "AzureServiceBus.Connectivity.MessageSenders.BrokerSideSubscriptionFilterFactoryInstance";
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
            public const string TransportType = "AzureServiceBus.Connectivity.TransportType";

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
            public const string MainSerializerSettingsKey = "MainSerializer";
            public const string RecoverabilityNumberOfImmediateRetries = "Recoverability.Immediate.Retries";
        }

        public static class Serialization
        {
            public const string BrokeredMessageBodyType = "AzureServiceBus.Serialization.BrokeredMessageBodyType";
        }
    }
}