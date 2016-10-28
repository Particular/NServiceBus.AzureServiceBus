namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusIndividualizationSettings : ExposeSettings
    {
        SettingsHolder settings;

        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public AzureServiceBusIndividualizationSettings(SettingsHolder settings) : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Provide individualization strategy to use.
        /// <remarks>Default is <see cref="DiscriminatorBasedIndividualization"/></remarks>
        /// <seealso cref="CoreIndividualization"/>
        /// <seealso cref="DiscriminatorBasedIndividualization"/>
        /// </summary>
        public AzureServiceBusIndividualizationExtensionPoint<T> UseStrategy<T>() where T : IIndividualizationStrategy
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy, typeof(T));

            return new AzureServiceBusIndividualizationExtensionPoint<T>(settings);
        }
    }
}