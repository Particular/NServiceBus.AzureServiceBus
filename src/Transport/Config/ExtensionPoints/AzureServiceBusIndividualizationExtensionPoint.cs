namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusIndividualizationExtensionPoint<T> : ExposeSettings where T : IIndividualizationStrategy
    {
        internal AzureServiceBusIndividualizationExtensionPoint(SettingsHolder settings, T strategy) : base(settings)
        {
            Strategy = strategy;
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy, Strategy);
        }

        public T Strategy { get; }
    }
}