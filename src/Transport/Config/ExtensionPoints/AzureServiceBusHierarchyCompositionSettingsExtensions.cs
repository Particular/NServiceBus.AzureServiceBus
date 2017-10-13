namespace NServiceBus
{
    using System;
    using Configuration.AdvancedExtensibility;
    using Transport.AzureServiceBus;

    /// <summary><see cref="HierarchyComposition"/> specific settings.</summary>
    public static class AzureServiceBusHierarchyCompositionSettingsExtensions
    {
        /// <summary>
        /// Path generator to be used to determine path from entity path/name.
        /// </summary>
        public static AzureServiceBusCompositionExtensionPoint<HierarchyComposition> PathGenerator(this AzureServiceBusCompositionExtensionPoint<HierarchyComposition> composition, Func<string, string> pathGenerator)
        {
            composition.GetSettings().Set(WellKnownConfigurationKeys.Topology.Addressing.Composition.HierarchyCompositionPathGenerator, pathGenerator);
            return composition;
        }
    }
}