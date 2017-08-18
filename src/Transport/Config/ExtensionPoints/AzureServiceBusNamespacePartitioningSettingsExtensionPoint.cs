namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusNamespacePartitioningSettingsExtensionPoint<T> : ExposeSettings where T : INamespacePartitioningStrategy
    {
        internal AzureServiceBusNamespacePartitioningSettingsExtensionPoint(SettingsHolder settings, T strategy) : base(settings)
        {
            Strategy = strategy;
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, strategy);
        }

        public T Strategy { get; }
    }
}