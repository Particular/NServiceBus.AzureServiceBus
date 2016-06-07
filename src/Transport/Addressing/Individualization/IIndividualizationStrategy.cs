namespace NServiceBus.AzureServiceBus.Addressing
{
    public interface IIndividualizationStrategy
    {
        string Individualize(string endpointName);
    }
}
