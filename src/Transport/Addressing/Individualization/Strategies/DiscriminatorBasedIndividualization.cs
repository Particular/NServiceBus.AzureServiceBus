namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using Settings;

    public class DiscriminatorBasedIndividualization : IIndividualizationStrategy
    {
        ReadOnlySettings settings;

        public DiscriminatorBasedIndividualization(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public string Individualize(string endpointName)
        {
            Func<string, string> discriminatorGenerator;
            var found = settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Individualization.DiscriminatorBasedIndividualizationDiscriminatorGenerator, out discriminatorGenerator);
            if (found == false)
            {
                var strategyName = typeof(DiscriminatorBasedIndividualization).Name;
                throw new Exception($"{strategyName} required discrimination generator to be registered. Use `.UseStrategy<{strategyName}>().DiscriminatorGenerator()` configuration API to register discrimination generator.");
            }

            var discriminator = discriminatorGenerator(endpointName);

            if (endpointName.EndsWith(discriminator))
            {
                return endpointName;
            }

            return endpointName + discriminator;
        }
    }
}