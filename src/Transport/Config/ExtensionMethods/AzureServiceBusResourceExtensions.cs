namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusResourceExtensions
    {
        public static AzureServiceBusQueueSettings Queues(this AzureServiceBusResourceSettings resourceSettings)
        {
            return new AzureServiceBusQueueSettings(resourceSettings.GetSettings());
        }

        public static AzureServiceBusTopicSettings Topics(this AzureServiceBusResourceSettings resourceSettings)
        {
            return new AzureServiceBusTopicSettings(resourceSettings.GetSettings());
        }

        public static AzureServiceBusSubscriptionSettings Subscriptions(this AzureServiceBusResourceSettings resourceSettings)
        {
            return new AzureServiceBusSubscriptionSettings(resourceSettings.GetSettings());
        }
    }

}
