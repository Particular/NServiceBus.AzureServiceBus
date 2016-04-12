namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;

    public class DiscriminatorBasedIndividualization : IIndividualizationStrategy
    {
        Func<string> discriminatorGenerator;

        public void SetDiscriminatorGenerator(Func<string> discriminatorGenerator)
        {
            this.discriminatorGenerator = discriminatorGenerator;
        }

        public string Individualize(string endpointname)
        {
            var discriminator = discriminatorGenerator();

            if (endpointname.EndsWith(discriminator))
                return endpointname;

            return endpointname + discriminator;
        }
    }
}