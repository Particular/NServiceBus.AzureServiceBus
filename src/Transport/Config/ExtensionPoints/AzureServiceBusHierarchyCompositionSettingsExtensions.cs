namespace NServiceBus
{
    using System;
    using AzureServiceBus.Addressing;
    using Configuration.AdvanceExtensibility;

    public static class AzureServiceBusHierarchyCompositionSettingsExtensions
    {
        public static AzureServiceBusCompositionExtensionPoint<HierarchyComposition> PathGenerator(this AzureServiceBusCompositionExtensionPoint<HierarchyComposition> composition, Func<string, string> pathGenerator)
        {
            composition.GetSettings().Set(HierarchyComposition.PathGeneratorSettingsKey, pathGenerator);
            return composition;
        }
    }
}