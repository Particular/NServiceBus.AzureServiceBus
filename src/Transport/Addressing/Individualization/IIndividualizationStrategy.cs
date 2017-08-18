namespace NServiceBus.Transport.AzureServiceBus
{
    using Settings;

    public interface IIndividualizationStrategy
    {
        void Initialize(ReadOnlySettings settings);

        string Individualize(string endpointName);
    }
}