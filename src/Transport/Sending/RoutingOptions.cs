namespace NServiceBus.Transport.AzureServiceBus
{
    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class RoutingOptions
    {
        public string DestinationEntityPath { get; set; }
        public RuntimeNamespaceInfo DestinationNamespace { get; set; }

        public bool SendVia { get; set; }
        public string ViaEntityPath { get; set; }
        public string ViaConnectionString { get; set; }
        public string ViaPartitionKey { get; set; }
    }
}