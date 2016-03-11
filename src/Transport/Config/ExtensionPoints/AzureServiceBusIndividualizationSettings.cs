namespace NServiceBus
{
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusIndividualizationSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusIndividualizationSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Provide individualization strategy to use.
        /// <remarks>Default is <see cref="DiscriminatorBasedIndividualization"/></remarks>
        /// <seealso cref="CoreIndividualization"/>
        /// </summary>
        public AzureServiceBusIndividualizationSettings UseStrategy<T>() where T : IIndividualizationStrategy
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy, typeof(T));

            return this;
        }
    }
}