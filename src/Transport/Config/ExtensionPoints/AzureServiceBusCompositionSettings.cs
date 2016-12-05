namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusCompositionSettings : ExposeSettings
    {
        SettingsHolder settings;

        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public AzureServiceBusCompositionSettings(SettingsHolder settings) : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Provide composition strategy to use.
        /// <remarks>Default is <see cref="FlatComposition"/></remarks>
        /// <seealso cref="HierarchyComposition"/>
        /// </summary>
        public AzureServiceBusCompositionExtensionPoint<T> UseStrategy<T>() where T : ICompositionStrategy
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy, typeof(T));

            return new AzureServiceBusCompositionExtensionPoint<T>(settings);
        }
    }
}