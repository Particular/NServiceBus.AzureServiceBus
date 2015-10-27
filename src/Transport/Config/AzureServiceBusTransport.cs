namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    public class AzureServiceBusTransport : TransportDefinition
    {
        protected override void ConfigureForReceiving(TransportReceivingConfigurationContext context)
        {
            throw new NotImplementedException();
        }

        protected override void ConfigureForSending(TransportSendingConfigurationContext context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Type> GetSupportedDeliveryConstraints()
        {
            throw new NotImplementedException();
        }

        public override TransactionSupport GetTransactionSupport()
        {
            throw new NotImplementedException();
        }

        public override IManageSubscriptions GetSubscriptionManager()
        {
            throw new NotImplementedException();
        }

        public override string GetDiscriminatorForThisEndpointInstance()
        {
            throw new NotImplementedException();
        }

        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            throw new NotImplementedException();
        }

        public override OutboundRoutingPolicy GetOutboundRoutingPolicy(ReadOnlySettings settings)
        {
            throw new NotImplementedException();
        }

        public override string ExampleConnectionStringForErrorMessage { get; }
    }
}