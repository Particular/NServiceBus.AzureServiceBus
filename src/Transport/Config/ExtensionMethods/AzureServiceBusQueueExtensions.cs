namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusQueueExtensions
    {
        public static AzureServiceBusQueueSettings SupportOrdering(this AzureServiceBusQueueSettings resourceSettings, bool supported)
        {
            resourceSettings.GetSettings().Set(WellKnownConfigurationKeys.Topology.Resources.Queues.SupportOrdering, supported);

            return resourceSettings;
        }
    }
}