namespace NServiceBus
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusIndividualizationExtensions
    {
        public static AzureServiceBusIndividualizationSettings Strategy<T>(this AzureServiceBusIndividualizationSettings individualizationSettings) where T : IIndividualizationStrategy
        {
            individualizationSettings.GetSettings().Set(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy, typeof(T));

            return individualizationSettings;
        }
    }
}