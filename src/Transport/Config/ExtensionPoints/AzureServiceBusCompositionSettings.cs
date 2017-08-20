namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public partial class AzureServiceBusCompositionSettings : ExposeSettings
    {
        internal AzureServiceBusCompositionSettings(SettingsHolder settings) : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Provide composition strategy to use.
        /// <remarks>Default is <see cref="FlatComposition" /></remarks>
        /// <seealso cref="HierarchyComposition" />
        /// </summary>
        public AzureServiceBusCompositionExtensionPoint<T> UseStrategy<T>(T strategy) where T : ICompositionStrategy
        {
            return new AzureServiceBusCompositionExtensionPoint<T>(settings, strategy);
        }

        SettingsHolder settings;
    }
}