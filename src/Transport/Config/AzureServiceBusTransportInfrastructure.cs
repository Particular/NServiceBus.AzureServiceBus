namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.Performance.TimeToBeReceived;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    public class AzureServiceBusTransportInfrastructure : TransportInfrastructure
    {
        private readonly ITopology topology;

        public AzureServiceBusTransportInfrastructure(ITopology topology, SettingsHolder settings)
        {
            this.topology = topology;
        }

        public override EndpointInstance BindToLocalEndpoint(EndpointInstance instance)
        {
            return instance;
        }

        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            return logicalAddress.ToString();
        }

        public override TransportReceiveInfrastructure ConfigureReceiveInfrastructure()
        {
            return new TransportReceiveInfrastructure(
                topology.GetMessagePumpFactory(),
                topology.GetQueueCreatorFactory(),
                () => Task.FromResult(StartupCheckResult.Success));
        }

        public override TransportSendInfrastructure ConfigureSendInfrastructure()
        {
            return new TransportSendInfrastructure(
                topology.GetDispatcherFactory(),
                () => Task.FromResult(StartupCheckResult.Success));
        }

        public override TransportSubscriptionInfrastructure ConfigureSubscriptionInfrastructure()
        {
            return new TransportSubscriptionInfrastructure(topology.GetSubscriptionManagerFactory());
        }

        public override IEnumerable<Type> DeliveryConstraints
        {
            get
            {
                return new List<Type> { typeof(DelayDeliveryWith), typeof(DoNotDeliverBefore), typeof(DiscardIfNotReceivedBefore) };
            }
        }

        public override TransportTransactionMode TransactionMode
        {
            get
            {
                return TransportTransactionMode.SendsAtomicWithReceive; // todo make dependant on #namespaces
            }
        }

        public override OutboundRoutingPolicy OutboundRoutingPolicy
        {
            get
            {
                return topology.GetOutboundRoutingPolicy();
            }
        }
        
    }
}