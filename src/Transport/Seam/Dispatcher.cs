namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Extensibility;
    using NServiceBus.Logging;
    using NServiceBus.Transports;

    class Dispatcher : IDispatchMessages
    {
        ILog logger = LogManager.GetLogger<Dispatcher>();
        readonly Batcher batcher;

        public Dispatcher(IRouteOutgoingMessages routeOutgoingMessages)
        {
            batcher = new Batcher(routeOutgoingMessages);
        }

        public Task Dispatch(IEnumerable<TransportOperation> outgoingMessages, ReadOnlyContextBag context)
        {
            return batcher.SendInBatches(outgoingMessages);
        }
    }


}