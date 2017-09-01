namespace NServiceBus
{
    using System;
    using Configuration.AdvancedExtensibility;
    using Transport.AzureServiceBus;

    public static class AzureServiceBusDiscriminatorBasedIndividualizationSettingsExtensions
    {
        /// <summary>
        /// Set discriminator generator for <see cref="DiscriminatorBasedIndividualization" /> individualization strategy.
        /// </summary>
        /// <param name="individualizationStrategy">DiscriminatorBasedIndividualization</param>
        /// <param name="discriminatorGenerator">
        /// Generator function that receives an endpoint name and returns individualized
        /// endpoint name.
        /// </param>
        public static AzureServiceBusIndividualizationExtensionPoint<DiscriminatorBasedIndividualization> DiscriminatorGenerator(this AzureServiceBusIndividualizationExtensionPoint<DiscriminatorBasedIndividualization> individualizationStrategy, Func<string, string> discriminatorGenerator)
        {
            individualizationStrategy.GetSettings().Set(WellKnownConfigurationKeys.Topology.Addressing.Individualization.DiscriminatorBasedIndividualizationDiscriminatorGenerator, discriminatorGenerator);

            return individualizationStrategy;
        }
    }
}