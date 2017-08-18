namespace NServiceBus.Transport.AzureServiceBus
{
    public interface IIndividualizationStrategy
    {
        string Individualize(string endpointName);
    }
}