namespace NServiceBus.AzureServiceBus.Addressing
{
    public class CoreIndividualization : IIndividualizationStrategy
    {
        public string Individualize(string endpointname)
        {
            return endpointname;
        }
    }
}