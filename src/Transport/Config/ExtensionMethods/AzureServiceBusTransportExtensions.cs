namespace NServiceBus
{
    using System;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusTransportExtensions 
    {
        public static AzureServiceBusTopologySettings UseTopology<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions) where T : ITopology, new()
        {
            var topology = Activator.CreateInstance<T>();
            return UseTopology(transportExtensions, topology);
        }

        public static AzureServiceBusTopologySettings UseTopology<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions, Func<T> factory) where T : ITopology
        {
            return UseTopology(transportExtensions, factory());
        }

        public static AzureServiceBusTopologySettings UseTopology<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions, T topology) where T : ITopology
        {
            var settings = transportExtensions.GetSettings();
            settings.Set<ITopology>(topology);
            return new AzureServiceBusTopologySettings(settings);
        }

        public static AzureServiceBusTopologySettings UseDefaultTopology(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
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