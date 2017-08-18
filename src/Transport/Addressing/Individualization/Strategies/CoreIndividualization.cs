namespace NServiceBus
{
    using Settings;
    using Transport.AzureServiceBus;

    public class CoreIndividualization : IIndividualizationStrategy
    {
        public void Initialize(ReadOnlySettings settings)
        {
        }

        public string Individualize(string endpointName)
        {
            return endpointName;
        }
    }
}