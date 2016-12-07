namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Transport.AzureServiceBus;

    public static partial class AzureServiceBusForwardingTopologySettingsExtensions
    {
        public static AzureServiceBusForwardingTopologySettings NumberOfEntitiesInBundle(this AzureServiceBusForwardingTopologySettings topologySettings, int number)
        {
            var settings = topologySettings.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle, number);
            return topologySettings;
        }

        public static AzureServiceBusForwardingTopologySettings BundlePrefix(this AzureServiceBusForwardingTopologySettings topologySettings, string prefix)
        {
            var settings = topologySettings.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix, prefix);
            return topologySettings;
        }
    }
}