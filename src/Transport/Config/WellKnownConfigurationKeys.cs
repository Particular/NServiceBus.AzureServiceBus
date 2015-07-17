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
                    public const string SupportOrdering = "AzureServiceBus.Settings.Topology.Resources.Queues.SupportOrdering";
                }
                public static class Topics
                {
                    public const string SupportOrdering = "AzureServiceBus.Settings.Topology.Resources.Topics.SupportOrdering";
                }
                public static class Subscriptions
                {
                    public const string SupportOrdering = "AzureServiceBus.Settings.Topology.Resources.Subscriptions.SupportOrdering";
                }
            }
        }

        
    }
}