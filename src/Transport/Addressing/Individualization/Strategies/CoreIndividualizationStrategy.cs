namespace NServiceBus.AzureServiceBus.Addressing
{
    public class CoreIndividualizationStrategy : IIndividualizationStrategy
    {
        public string Individualize(string endpointname)
        {
            return endpointname;
        }
    }
}