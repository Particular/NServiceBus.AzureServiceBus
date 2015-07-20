namespace NServiceBus
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusCompositionExtensions
    {
        public static AzureServiceBusCompositionSettings Strategy<T>(this AzureServiceBusCompositionSettings compositionSettings) where T : ICompositionStrategy
        {
            compositionSettings.GetSettings().Set(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy, typeof(T));

            return compositionSettings;
        }
    }
}