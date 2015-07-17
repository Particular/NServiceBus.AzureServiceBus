namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusTopicExtensions
    {
        public static AzureServiceBusTopicSettings SupportOrdering(this AzureServiceBusTopicSettings resourceSettings, bool supported)
        {
            resourceSettings.GetSettings().Set(WellKnownConfigurationKeys.Topology.Resources.Topics.SupportOrdering, supported);

            return resourceSettings;
        }
    }
}