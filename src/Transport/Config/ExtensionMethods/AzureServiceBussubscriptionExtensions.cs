namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusSubscriptionExtensions
    {
        public static AzureServiceBusSubscriptionSettings SupportOrdering(this AzureServiceBusSubscriptionSettings resourceSettings, bool supported)
        {
            resourceSettings.GetSettings().Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.SupportOrdering, supported);

            return resourceSettings;
        }
    }
}