namespace NServiceBus
{
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusCompositionSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusCompositionSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Provide composition strategy to use.
        /// <remarks>Default is <see cref="FlatComposition"/></remarks>
        /// <seealso cref="HierarchyComposition"/>
        /// </summary>
        public AzureServiceBusCompositionSettings UseStrategy<T>() where T : ICompositionStrategy
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy, typeof(T));

            return this;
        }
    }
}