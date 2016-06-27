namespace NServiceBus
{
    using Transport.AzureServiceBus;

    public class CoreIndividualization : IIndividualizationStrategy
    {
        public string Individualize(string endpointName)
        {
            return endpointName;
        }
    }
}