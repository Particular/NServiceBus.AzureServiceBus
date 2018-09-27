namespace NServiceBus
{
    using Configuration.AdvancedExtensibility;
    using Transport.AzureServiceBus;

    /// <summary>
    /// Settings for the forwarding topology
    /// </summary>
    public static class AzureServiceBusForwardingTopologySettingsExtensions
    {
        /// <summary>
        /// Allows to set the bundle prefix
        /// </summary>
        /// <param name="topologySettings"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static AzureServiceBusForwardingTopologySettings BundlePrefix(this AzureServiceBusForwardingTopologySettings topologySettings, string prefix)
        {
            var settings = topologySettings.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix, prefix);
            return topologySettings;
        }
    }
}