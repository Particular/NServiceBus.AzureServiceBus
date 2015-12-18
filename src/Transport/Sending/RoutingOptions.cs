namespace NServiceBus.AzureServiceBus
{
    public class RoutingOptions
    {
        public bool SendVia { get; set; }
        public string ViaEntityPath { get; set; }
        public string ViaConnectionString { get; set; }
        public string ViaPartitionKey { get; set; }
    }
}