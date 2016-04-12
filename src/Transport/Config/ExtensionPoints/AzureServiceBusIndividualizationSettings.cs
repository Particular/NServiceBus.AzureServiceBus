namespace NServiceBus
{
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using Configuration.AdvanceExtensibility;
    using Settings;

    public class AzureServiceBusIndividualizationSettings : ExposeSettings
    {
        SettingsHolder settings;

        public AzureServiceBusIndividualizationSettings(SettingsHolder settings)
            : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Provide individualization strategy to use.
        /// <remarks>Default is <see cref="DiscriminatorBasedIndividualization"/></remarks>
        /// <seealso cref="CoreIndividualization"/>
        /// </summary>
        public AzureServiceBusIndividualizationSettings UseStrategy<T>() where T : IIndividualizationStrategy
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy, typeof(T));

            return this;
        }
    }
}