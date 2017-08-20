namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusCompositionExtensionPoint<T> : ExposeSettings where T : ICompositionStrategy
    {
        internal AzureServiceBusCompositionExtensionPoint(SettingsHolder settings, T strategy) : base(settings)
        {
            Strategy = strategy;
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy, Strategy);
        }

        public T Strategy { get; }
    }
}