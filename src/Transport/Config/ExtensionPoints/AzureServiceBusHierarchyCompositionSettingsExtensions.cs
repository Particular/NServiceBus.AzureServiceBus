namespace NServiceBus
{
    using System;
    using Configuration.AdvancedExtensibility;
    using Transport.AzureServiceBus;

    public static class AzureServiceBusHierarchyCompositionSettingsExtensions
    {
        public static AzureServiceBusCompositionExtensionPoint<HierarchyComposition> PathGenerator(this AzureServiceBusCompositionExtensionPoint<HierarchyComposition> composition, Func<string, string> pathGenerator)
        {
            composition.GetSettings().Set(WellKnownConfigurationKeys.Topology.Addressing.Composition.HierarchyCompositionPathGenerator, pathGenerator);
            return composition;
        }
    }
}