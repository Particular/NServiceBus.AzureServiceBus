namespace NServiceBus
{
    using Transport.AzureServiceBus;

    
    public class CoreIndividualization : IIndividualizationStrategy
    {
        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public CoreIndividualization()
        {
        }

        public string Individualize(string endpointName)
        {
            return endpointName;
        }
    }
}