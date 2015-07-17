namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusTransportExtensions
    {
        public static AzureServiceBusTopologySettings Topology(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusTopologySettings(transportExtensions.GetSettings());
        }
    }
}