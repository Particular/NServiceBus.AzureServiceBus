namespace NServiceBus.AzureServiceBus
{
    using NServiceBus.Transports;

    public class RoutingOptions
    {
        public bool SendVia { get; set; }
        public string ViaEntityPath { get; set; }
        public string ViaConnectionString { get; set; }
        public DispatchOptions DispatchOptions { get; set; }
    }
}