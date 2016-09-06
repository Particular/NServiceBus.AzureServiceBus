namespace NServiceBus.Transport.AzureServiceBus
{ 
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