namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public partial class AzureServiceBusIndividualizationSettings : ExposeSettings
    {
        internal AzureServiceBusIndividualizationSettings(SettingsHolder settings) : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Provide individualization strategy to use.
        /// <remarks>Default is <see cref="DiscriminatorBasedIndividualization" /></remarks>
        /// <seealso cref="CoreIndividualization" />
        /// <seealso cref="DiscriminatorBasedIndividualization" />
        /// </summary>
        public AzureServiceBusIndividualizationExtensionPoint<T> UseStrategy<T>(T strategy) where T : IIndividualizationStrategy
        {
            return new AzureServiceBusIndividualizationExtensionPoint<T>(settings, strategy);
        }

        SettingsHolder settings;
    }
}