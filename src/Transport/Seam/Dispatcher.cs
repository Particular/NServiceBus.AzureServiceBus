namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Transports;

    class Dispatcher : IDispatchMessages
    {
        public Task Dispatch(IEnumerable<TransportOperation> outgoingMessages)
        {
            throw new NotImplementedException();
        }
    }
}