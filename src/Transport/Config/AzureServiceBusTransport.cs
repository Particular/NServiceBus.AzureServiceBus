namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.Performance.TimeToBeReceived;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    public class AzureServiceBusTransport : TransportDefinition
    {
        protected override void ConfigureForReceiving(TransportReceivingConfigurationContext context)
        {
            //context.SetQueueCreatorFactory();
            //context.SetMessagePumpFactory();
        }

        protected override void ConfigureForSending(TransportSendingConfigurationContext context)
        {
            throw new NotImplementedException();
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
            // TODO: decision is based on type of Topology (BasicTopology doesn't have pub/sub).
            // TODO: need configuration object here to make the decision
            return TransactionSupport.MultiQueue;
        }

        public override IManageSubscriptions GetSubscriptionManager()
        {
            // TODO: decision is based on type of Topology (BasicTopology doesn't have pub/sub).
            // TODO: need configuration object here to make the decision
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override OutboundRoutingPolicy GetOutboundRoutingPolicy(ReadOnlySettings settings)
        {
            // TODO: decision is based on type of Topology (BasicTopology doesn't have pub/sub).
            // TODO: need a container here
            // return new OutboundRoutingPolicy(OutboundRoutingType.DirectSend, OutboundRoutingType.DirectSend, OutboundRoutingType.DirectSend); // BasicTopology
            return new OutboundRoutingPolicy(OutboundRoutingType.DirectSend, OutboundRoutingType.IndirectPublish, OutboundRoutingType.DirectSend); // StandardTopology
        }

        public override string ExampleConnectionStringForErrorMessage { get; } = "Endpoint=sb://[namespace].servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=[secret_key]";
    }
}