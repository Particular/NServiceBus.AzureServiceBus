namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;

    public class DiscriminatorBasedIndividualization : IIndividualizationStrategy
    {
        Func<string> _discriminatorGenerator;

        public void SetDiscriminatorGenerator(Func<string> discriminatorGenerator)
        {
            _discriminatorGenerator = discriminatorGenerator;
        }

        public string Individualize(string endpointname)
        {
            var discriminator = _discriminatorGenerator();

            if (endpointname.EndsWith(discriminator))
                return endpointname;

            return endpointname + discriminator;
        }
    }
}