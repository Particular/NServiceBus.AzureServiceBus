namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.Features;
    using NServiceBus.Performance.TimeToBeReceived;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    public class AzureServiceBusTransport : TransportDefinition
    {
        protected override TransportReceivingConfigurationResult ConfigureForReceiving(TransportReceivingConfigurationContext context)
        {
            EnsureConnectionStringIsRegisteredAsNamespace(context.ConnectionString, context.Settings);
            return new TransportReceivingConfigurationResult(
                Topology.GetMessagePumpFactory(),
                Topology.GetQueueCreatorFactory(),
                () => Task.FromResult(StartupCheckResult.Success) //TODO: figure out what this is for
                );
        }

        protected override TransportSendingConfigurationResult ConfigureForSending(TransportSendingConfigurationContext context)
        {
            EnsureConnectionStringIsRegisteredAsNamespace(context.ConnectionString, context.Settings);

            return new TransportSendingConfigurationResult(
                Topology.GetDispatcherFactory(),
                () => Task.FromResult(StartupCheckResult.Success) //TODO: figure out what this is for
                );
        }

        private void EnsureConnectionStringIsRegisteredAsNamespace(string connectionstring, ReadOnlySettings settings)
        {
            var namespaces = settings.Get<List<string>>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            if (namespaces.All(n => n != connectionstring))
            {
                namespaces.Add(connectionstring);
            }
        }

        public override IEnumerable<Type> GetSupportedDeliveryConstraints()
        {
            return new List<Type>
            {
                typeof(DelayDeliveryWith),
                typeof(DoNotDeliverBefore),
                typeof(DiscardIfNotReceivedBefore),
            };
        }

        public override TransportTransactionMode GetSupportedTransactionMode()
        {
            // TODO: See where Core is calling this and make sure None is the correct value.
            // TODO: TransportTransactionMode may need to be dependent upon topology.
            return TransportTransactionMode.None;
        }


        public override IManageSubscriptions GetSubscriptionManager()
        {
            return Topology.GetSubscriptionManager();
        }

        public override string GetDiscriminatorForThisEndpointInstance(ReadOnlySettings settings)
        {
            // Decision is based on 3 parties: Core + Transport + Host + User scenario (based on hosting scenario, ex: multiple instances on the same physical machine)
            // TODO: What is "discriminator"? Does that include endpoint instance identifier or not?

            // var instanceId = settings.Get<InstanceId>(); // instance id set by the host
            // var discriminator = "-" + instanceId;
            // return Transport.ValidateDiscriminator(discriminator);
            return null;
        }

        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            // ???
            return logicalAddress.ToString();
        }

        public override OutboundRoutingPolicy GetOutboundRoutingPolicy(ReadOnlySettings settings)
        {
            // TODO: remove if when support for testing multiple topologies is supported by ATT framework
            // need a way to specify what topology should be used before endpoint is started
            if (Topology == null)
            {
                Topology = new StandardTopology();
            }
            return Topology.GetOutboundRoutingPolicy();
        }

        public override string ExampleConnectionStringForErrorMessage { get; } = "Endpoint=sb://[namespace].servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=[secret_key]";

        private ITopology _topology ;   
        internal ITopology Topology
        {
            get { return _topology; }
            set
            {
                _topology = value;
            }
        }
    }

    public class AzureServiceBusTransportConfigurator : Feature
    {
        private SettingsHolder settings { get; set; }

        internal AzureServiceBusTransportConfigurator()
        {
            EnableByDefault();
            //DependsOn<UnicastBus>();
            //DependsOn<Receiving>();
            Defaults(settings =>
            {
                this.settings = settings;
                settings.SetDefault("Transactions.DoNotWrapHandlersExecutionInATransactionScope", true);
                settings.SetDefault("Transactions.SuppressDistributedTransactions", true);

                var transportDefinition = (AzureServiceBusTransport) settings.Get<TransportDefinition>();
                if (transportDefinition.Topology == null)
                {
                    transportDefinition.Topology = new StandardTopology();
                }
                transportDefinition.Topology.Initialize(settings);
            });
        }

        


        protected override void Setup(FeatureConfigurationContext context)
        {
            //context.Container //can only register
            //context.Pipeline //can extend
            //context.Settings //cannot change
        }


    }

}