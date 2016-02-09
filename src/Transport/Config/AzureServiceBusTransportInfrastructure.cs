namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        private readonly SettingsHolder settings;

        public AzureServiceBusTransportInfrastructure(ITopology topology, SettingsHolder settings)
        {
            this.topology = topology;
            this.settings = settings;
        }

        public override EndpointInstance BindToLocalEndpoint(EndpointInstance instance, ReadOnlySettings settings)
        {
            return instance;
        }

        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            return logicalAddress.ToString();
        }

        public override TransportReceiveInfrastructure ConfigureReceiveInfrastructure(string connectionString)
        {
            EnsureConnectionStringIsRegisteredAsNamespace(connectionString, settings);
            return new TransportReceiveInfrastructure(
                topology.GetMessagePumpFactory(),
                topology.GetQueueCreatorFactory(),
                () => Task.FromResult(StartupCheckResult.Success));
        }

        public override TransportSendInfrastructure ConfigureSendInfrastructure(string connectionString)
        {
            EnsureConnectionStringIsRegisteredAsNamespace(connectionString, settings);
            return new TransportSendInfrastructure(
                topology.GetDispatcherFactory(),
                () => Task.FromResult(StartupCheckResult.Success));
        }

        public override TransportSubscriptionInfrastructure ConfigureSubscriptionInfrastructure()
        {
            return new TransportSubscriptionInfrastructure(topology.GetSubscriptionManagerFactory());
        }

        public override string ExampleConnectionStringForErrorMessage { get; } = "Endpoint=sb://[namespace].servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=[secret_key]";

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

        private void EnsureConnectionStringIsRegisteredAsNamespace(string connectionstring, ReadOnlySettings settings)
        {
            var namespaces = settings.Get<List<string>>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            if (namespaces.All(n => n != connectionstring))
            {
                namespaces.Add(connectionstring);
            }
        }
    }
}