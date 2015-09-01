namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusTransportExtensions
    {
        public static AzureServiceBusTopologySettings Topology(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusTopologySettings(transportExtensions.GetSettings());
        }

        public static AzureServiceBusBatchingSettings Batching(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusBatchingSettings(transportExtensions.GetSettings());
        }

        public static AzureServiceBusTransactionSettings Transactions(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusTransactionSettings(transportExtensions.GetSettings());
        }

        public static AzureServiceBusConnectivitySettings Connectivity(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusConnectivitySettings(transportExtensions.GetSettings());
        }

        public static AzureServiceBusSerializationSettings Serialization(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusSerializationSettings(transportExtensions.GetSettings());
        }

    }
}