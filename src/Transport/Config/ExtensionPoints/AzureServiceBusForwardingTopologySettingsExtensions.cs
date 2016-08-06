namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Transport.AzureServiceBus;

    public static class AzureServiceBusForwardingTopologySettingsExtensions
    {
        public static AzureServiceBusTopologySettings<ForwardingTopology> NumberOfEntitiesInBundle(this AzureServiceBusTopologySettings<ForwardingTopology> topologySettings, int number)
        {
            var settings = topologySettings.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle, number);
            return topologySettings;
        }

        public static AzureServiceBusTopologySettings<ForwardingTopology> BundlePrefix(this AzureServiceBusTopologySettings<ForwardingTopology> topologySettings, string prefix)
        {
            var settings = topologySettings.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix, prefix);
            return topologySettings;
        }
    }
}