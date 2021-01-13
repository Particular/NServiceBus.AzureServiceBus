namespace NServiceBus
{
    using System;
    using Settings;
    using Transport.AzureServiceBus;

    /// <summary>
    ///  Strategy to modify the name of the endpoint by appending a discriminator value to the end.
    /// </summary>
    public class DiscriminatorBasedIndividualization : IIndividualizationStrategy
    {
        internal DiscriminatorBasedIndividualization(ReadOnlySettings settings)
        {
            var found = settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Individualization.DiscriminatorBasedIndividualizationDiscriminatorGenerator, out discriminatorGenerator);
            if (found == false)
            {
                var strategyName = nameof(DiscriminatorBasedIndividualization);
                throw new Exception($"{strategyName} required discrimination generator to be registered. Use `.UseStrategy<{strategyName}>().DiscriminatorGenerator()` configuration API to register discrimination generator.");
            }
        }

        /// <summary>
        ///  Modifies the <param name="endpointName" /> of the endpoint by appending a discriminator value to the end.
        /// </summary>
        public string Individualize(string endpointName)
        {
            var discriminator = discriminatorGenerator(endpointName);

            if (endpointName.EndsWith(discriminator))
            {
                return endpointName;
            }

            return endpointName + discriminator;
        }

        Func<string, string> discriminatorGenerator;
    }
}