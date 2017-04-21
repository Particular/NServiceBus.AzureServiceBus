namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Transport.AzureServiceBus;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public static class AzureServiceBusForwardingTopologySettingsExtensions
    {
        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static AzureServiceBusTopologySettings<ForwardingTopology> NumberOfEntitiesInBundle(this AzureServiceBusTopologySettings<ForwardingTopology> topologySettings, int number)
        {
            var settings = topologySettings.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle, number);
            return topologySettings;
        }

        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static AzureServiceBusTopologySettings<ForwardingTopology> BundlePrefix(this AzureServiceBusTopologySettings<ForwardingTopology> topologySettings, string prefix)
        {
            var settings = topologySettings.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix, prefix);
            return topologySettings;
        }

        [ObsoleteEx(Message = "Number of topics in the bundle by default is 2. This setting will be removed in the next major version and number of topics used will be 1.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static AzureServiceBusForwardingTopologySettings NumberOfEntitiesInBundle(this AzureServiceBusForwardingTopologySettings topologySettings, int number)
        {
            var settings = topologySettings.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle, number);
            return topologySettings;
        }

        [ObsoleteEx(Message = "Bundle prefix will be replaced in the next major version.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static AzureServiceBusForwardingTopologySettings BundlePrefix(this AzureServiceBusForwardingTopologySettings topologySettings, string prefix)
        {
            var settings = topologySettings.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix, prefix);
            return topologySettings;
        }
    }
}