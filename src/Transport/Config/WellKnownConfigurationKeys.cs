// ReSharper disable MemberHidesStaticFromOuterClass
namespace NServiceBus
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
                    public const string ForwardDeadLetteredMessagesTo = "AzureServiceBus.Settings.Topology.Resources.Queues.ForwardDeadLetteredMessagesTo";
                    public const string ForwardTo = "AzureServiceBus.Settings.Topology.Resources.Queues.ForwardTo";
                    public const string IsAnonymousAccessible = "AzureServiceBus.Settings.Topology.Resources.Queues.IsAnonymousAccessible";
                }

                public static class Topics
                {
                    public const string DescriptionFactory = "AzureServiceBus.Settings.Topology.Resources.Topics.DescriptionFactory";
                    public const string SupportOrdering = "AzureServiceBus.Settings.Topology.Resources.Topics.SupportOrdering";
                }

                public static class Subscriptions
                {
                    public const string DescriptionFactory = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.DescriptionFactory";
                    public const string SupportOrdering = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.SupportOrdering";
                }

                
            }

            public static class Addressing
            {
                public const string Strategy = "AzureServiceBus.Settings.Topology.Addressing.Strategy";

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
            public const string NumberOfMessagingFactoriesPerNamespace = "AzureServiceBus.Connectivity.NumberOfMessagingFactoriesPerNamespace";
            public const string NumberOfMessageReceiversPerEntity = "AzureServiceBus.Connectivity.NumberOfMessageReceiversPerEntity";
            public const string MessagingFactorySettingsFactory = "AzureServiceBus.Connectivity.MessagingFactorySettingsFactory";
        }

    }
}