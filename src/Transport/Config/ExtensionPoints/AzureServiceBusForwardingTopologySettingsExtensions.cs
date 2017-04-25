namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Transport.AzureServiceBus;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public static class AzureServiceBusForwardingTopologySettingsExtensions
    {
        [ObsoleteEx(Message = ObsoleteMessages.NumberOfTopicsInTheBundleWillBeRemoved, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static AzureServiceBusTopologySettings<ForwardingTopology> NumberOfEntitiesInBundle(this AzureServiceBusTopologySettings<ForwardingTopology> topologySettings, int number)
        {
            var settings = topologySettings.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle, number);
            return topologySettings;
        }

        [ObsoleteEx(Message = ObsoleteMessages.NumberOfTopicsInTheBundleWillBeRemoved, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static AzureServiceBusForwardingTopologySettings NumberOfEntitiesInBundle(this AzureServiceBusForwardingTopologySettings topologySettings, int number)
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

        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static AzureServiceBusForwardingTopologySettings BundlePrefix(this AzureServiceBusForwardingTopologySettings topologySettings, string prefix)
        {
            var settings = topologySettings.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix, prefix);
            return topologySettings;
        }
    }
}