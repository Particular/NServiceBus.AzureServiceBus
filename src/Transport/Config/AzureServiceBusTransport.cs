namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.Features;
    using NServiceBus.Performance.TimeToBeReceived;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    public class AzureServiceBusTransport : TransportDefinition
    {
        protected override void ConfigureForReceiving(TransportReceivingConfigurationContext context)
        {
            EnsureConnectionStringIsRegisteredAsNamespace(context.ConnectionString, context.Settings);

            context.SetQueueCreatorFactory(Topology.GetQueueCreatorFactory());
            context.SetMessagePumpFactory(Topology.GetMessagePumpFactory());
        }

        protected override void ConfigureForSending(TransportSendingConfigurationContext context)
        {
            EnsureConnectionStringIsRegisteredAsNamespace(context.ConnectionString, context.GlobalSettings);

            context.SetDispatcherFactory(Topology.GetDispatcherFactory());
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

        public override TransactionSupport GetTransactionSupport()
        {
            // TODO: need to test this and see when invoked from the core
            // TODO: decision is based on condition (if multiple namespaces are used, we can only support single queue, otherwise multi queue).
            // TODO: need configuration object here to make the decision
            return TransactionSupport.MultiQueue;
        }

        public override IManageSubscriptions GetSubscriptionManager()
        {
            return Topology.GetSubscriptionManager();
        }

        public override string GetDiscriminatorForThisEndpointInstance()
        {
            // Decision is based on 3 parties: Core + Transport + Host + User scenario (based on hosting scenario, ex: multiple instances on the same physical machine)
            // TODO: What is "discriminator"? Does that include endpoint instance identifier or not?

            // var instanceId = settings.Get<InstanceId>(); // instance id set by the host
            // var discriminator = "-" + instanceId;
            // return Transport.ValidateDiscriminator(discriminator);
            return "-";
        }

        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            // ???
            return logicalAddress.EndpointInstanceName.ToString();
        }

        public override OutboundRoutingPolicy GetOutboundRoutingPolicy(ReadOnlySettings settings)
        {
            return Topology.GetOutboundRoutingPolicy();
        }

        public override string ExampleConnectionStringForErrorMessage { get; } = "Endpoint=sb://[namespace].servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=[secret_key]";

        internal ITopology Topology { get; set; }
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