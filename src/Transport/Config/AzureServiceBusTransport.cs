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

    public class AzureServiceBusTransport : TransportDefinition
    {

        protected override TransportInfrastructure Initialize(SettingsHolder settings)
        {
            settings.SetDefault("Transactions.DoNotWrapHandlersExecutionInATransactionScope", true);
            settings.SetDefault("Transactions.SuppressDistributedTransactions", true);
            settings.SetDefault<ITopology>(new StandardTopology());

            var topology = settings.Get<ITopology>();
            topology.Initialize(settings);

            var transactionMode = TransportTransactionMode.SendsAtomicWithReceive; // todo make dependant on #namespaces
            var outboundRoutingPolicy = topology.GetOutboundRoutingPolicy();
            var deliveryConstraints = new List<Type> { typeof(DelayDeliveryWith), typeof(DoNotDeliverBefore), typeof(DiscardIfNotReceivedBefore) };

            Func<string, TransportSendInfrastructure> configureSendInfrastructure = s =>
            {
                EnsureConnectionStringIsRegisteredAsNamespace(s, settings);
                return new TransportSendInfrastructure(
                    topology.GetDispatcherFactory(),
                   () => Task.FromResult(StartupCheckResult.Success));
            };

            Func<string, TransportReceiveInfrastructure> configureReceiveInfrastructure = s =>
            {
                EnsureConnectionStringIsRegisteredAsNamespace(s, settings);
                return new TransportReceiveInfrastructure(
                    topology.GetMessagePumpFactory(),
                    topology.GetQueueCreatorFactory(),
                () => Task.FromResult(StartupCheckResult.Success));
            };

            Func<TransportSubscriptionInfrastructure> configureSubscriptionInfrastructure = () =>
            {
                return new TransportSubscriptionInfrastructure(topology.GetSubscriptionManagerFactory());
            };

            var supported = new AzureServiceBusTransportInfrastructure(
                deliveryConstraints,
                transactionMode,
                outboundRoutingPolicy,
                configureSendInfrastructure,
                configureReceiveInfrastructure,
                configureSubscriptionInfrastructure
            );

            return supported;
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

    public class AzureServiceBusTransportInfrastructure : TransportInfrastructure
    {
        public AzureServiceBusTransportInfrastructure(IEnumerable<Type> deliveryConstraints, TransportTransactionMode transactionMode, OutboundRoutingPolicy outboundRoutingPolicy, Func<string, TransportSendInfrastructure> configureSendInfrastructure, Func<string, TransportReceiveInfrastructure> configureReceiveInfrastructure = null, Func<TransportSubscriptionInfrastructure> configureSubscriptionInfrastructure = null) : base(deliveryConstraints, transactionMode, outboundRoutingPolicy, configureSendInfrastructure, configureReceiveInfrastructure, configureSubscriptionInfrastructure)
        {
        }

        public override EndpointInstance BindToLocalEndpoint(EndpointInstance instance, ReadOnlySettings settings)
        {
            return instance;
        }

        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            return logicalAddress.ToString();
        }

        public override string ExampleConnectionStringForErrorMessage { get; } = "Endpoint=sb://[namespace].servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=[secret_key]";
    }

}