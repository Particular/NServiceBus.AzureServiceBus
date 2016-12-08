namespace NServiceBus
{
    using Transport.AzureServiceBus;

    
    public class CoreIndividualization : IIndividualizationStrategy
    {
        internal CoreIndividualization()
        {
        }

        public string Individualize(string endpointName)
        {
            return endpointName;
        }
    }
}