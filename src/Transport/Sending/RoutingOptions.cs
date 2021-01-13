namespace NServiceBus.Transport.AzureServiceBus
{
    class RoutingOptionsInternal
    {
        public string DestinationEntityPath { get; set; }
        public RuntimeNamespaceInfo DestinationNamespace { get; set; }

        public bool SendVia { get; set; }
        public string ViaEntityPath { get; set; }
        public string ViaConnectionString { get; set; }
        public string ViaPartitionKey { get; set; }
    }
}