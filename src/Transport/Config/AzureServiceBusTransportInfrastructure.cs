namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using DelayedDelivery;
    using NServiceBus.AzureServiceBus.Topology.MetaModel;
    using Performance.TimeToBeReceived;
    using Routing;
    using Settings;
    using Transport;

    class AzureServiceBusTransportInfrastructure : TransportInfrastructure
    {
        ITopology topology;
        SettingsHolder settings;
        SatelliteTransportAddressCollection satelliteTransportAddresses;

        public AzureServiceBusTransportInfrastructure(ITopology topology, SettingsHolder settings, SatelliteTransportAddressCollection satelliteTransportAddresses)
        {
            this.topology = topology;
            this.settings = settings;
            this.satelliteTransportAddresses = satelliteTransportAddresses;
        }

        public override EndpointInstance BindToLocalEndpoint(EndpointInstance instance)
        {
            return topology.BindToLocalEndpoint(instance);
        }

        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            var queue = new StringBuilder(logicalAddress.EndpointInstance.Endpoint);

            if (logicalAddress.EndpointInstance.Discriminator != null)
            {
                queue.Append("-" + logicalAddress.EndpointInstance.Discriminator);
            }

            if (logicalAddress.Qualifier != null)
            {
                queue.Append("." + logicalAddress.Qualifier);

                // satellite input queue, store it for message pump to be able to determine what pump is for satellites and what is the main pump
                satelliteTransportAddresses.Add(queue.ToString());
            }

            return queue.ToString();
        }

        public override TransportReceiveInfrastructure ConfigureReceiveInfrastructure()
        {
            return new TransportReceiveInfrastructure(
                topology.GetMessagePumpFactory(),
                topology.GetQueueCreatorFactory(),
                () => topology.RunPreStartupChecks());
        }

        public override TransportSendInfrastructure ConfigureSendInfrastructure()
        {
            return new TransportSendInfrastructure(
                topology.GetDispatcherFactory(),
                () => topology.RunPreStartupChecks());
        }

        public override TransportSubscriptionInfrastructure ConfigureSubscriptionInfrastructure()
        {
            return new TransportSubscriptionInfrastructure(topology.GetSubscriptionManagerFactory());
        }

        public override IEnumerable<Type> DeliveryConstraints => new List<Type> { typeof(DelayDeliveryWith), typeof(DoNotDeliverBefore), typeof(DiscardIfNotReceivedBefore) };

        public override TransportTransactionMode TransactionMode => settings.SupportedTransactionMode();

        public override OutboundRoutingPolicy OutboundRoutingPolicy => topology.GetOutboundRoutingPolicy();
    }
}