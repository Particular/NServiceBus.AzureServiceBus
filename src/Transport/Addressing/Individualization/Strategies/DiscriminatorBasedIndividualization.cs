namespace NServiceBus
{
    using System;
    using Settings;
    using Transport.AzureServiceBus;

    public class DiscriminatorBasedIndividualization : IIndividualizationStrategy
    {
        public void Initialize(ReadOnlySettings settings)
        {
            var found = settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Individualization.DiscriminatorBasedIndividualizationDiscriminatorGenerator, out discriminatorGenerator);
            if (found == false)
            {
                var strategyName = typeof(DiscriminatorBasedIndividualization).Name;
                throw new Exception($"{strategyName} required discrimination generator to be registered. Use `.UseStrategy<{strategyName}>().DiscriminatorGenerator()` configuration API to register discrimination generator.");
            }
        }

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