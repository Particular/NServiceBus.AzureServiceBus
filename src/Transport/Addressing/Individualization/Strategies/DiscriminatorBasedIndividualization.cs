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
            var discriminatorGenerator = settings.Get<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Individualization.DiscriminatorBasedIndividualizationDiscriminatorGenerator);

            var discriminator = discriminatorGenerator(endpointName);

            if (endpointName.EndsWith(discriminator))
            {
                return endpointName;
            }

            return endpointName + discriminator;
        }
    }
}