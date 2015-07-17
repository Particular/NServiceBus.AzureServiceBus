namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using NServiceBus.ConsistencyGuarantees;
    using NServiceBus.Transports;

    public class AzureServiceBusTransport : TransportDefinition
    {
        public override string GetSubScope(string address, string qualifier)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Type> GetSupportedDeliveryConstraints()
        {
            throw new NotImplementedException();
        }

        public override ConsistencyGuarantee GetDefaultConsistencyGuarantee()
        {
            throw new NotImplementedException();
        }
    }
}