namespace NServiceBus
{
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using Configuration.AdvanceExtensibility;
    using Settings;

    public class AzureServiceBusCompositionSettings : ExposeSettings
    {
        SettingsHolder settings;

        public AzureServiceBusCompositionSettings(SettingsHolder settings)
            : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Provide composition strategy to use.
        /// <remarks>Default is <see cref="FlatComposition"/></remarks>
        /// <seealso cref="HierarchyComposition"/>
        /// </summary>
        public AzureServiceBusCompositionSettings UseStrategy<T>() where T : ICompositionStrategy
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy, typeof(T));

            return this;
        }
    }
}