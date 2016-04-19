namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Extensibility;
    using Transports;

    class Dispatcher : IDispatchMessages
    {
        IRouteOutgoingBatches routeOutgoingBatches;
        IBatcher batcher;

        public Dispatcher(IRouteOutgoingBatches routeOutgoingBatches, IBatcher batcher)
        {
            this.routeOutgoingBatches = routeOutgoingBatches;
            this.batcher = batcher;
        }

        public Task Dispatch(TransportOperations operations, ContextBag context)
        {
            ReceiveContext receiveContext;
            var outgoingBatches = batcher.ToBatches(operations);

            context.TryGet(out receiveContext);
            if (receiveContext == null) // not in a receive context, so send out immediately
            {
                return routeOutgoingBatches.RouteBatches(outgoingBatches, receiveContext: null);
            }

            var brokeredMessageReceiveContext = receiveContext as BrokeredMessageReceiveContext;

            if (brokeredMessageReceiveContext != null) // apply brokered message specific dispatching rules
            {
                return DispatchBatches(outgoingBatches, brokeredMessageReceiveContext);
            }
            // case when the receive context is different from brokered messaging (like eventhub)

            return routeOutgoingBatches.RouteBatches(outgoingBatches, receiveContext); // otherwise send out immediately
        }

        Task DispatchBatches(IList<Batch> outgoingBatches, BrokeredMessageReceiveContext receiveContext)
        {
            // received brokered message has already been completed, so send everything out immediately
            if (receiveContext.ReceiveMode == ReceiveMode.ReceiveAndDelete)
            {
                return routeOutgoingBatches.RouteBatches(outgoingBatches, receiveContext);
            }
            // default behavior is to postpone sends until complete (and potentially complete them as a single tx if possible)
            // but some messages may need to go out immediately

            var toBeDispatchedImmediately = outgoingBatches.Where(t => t.RequiredDispatchConsistency == DispatchConsistency.Isolated);
            var toBeDispatchedOnComplete = outgoingBatches.Where(t => t.RequiredDispatchConsistency == DispatchConsistency.Default);

            receiveContext.OnComplete.Add(() => routeOutgoingBatches.RouteBatches(toBeDispatchedOnComplete, receiveContext));
            return routeOutgoingBatches.RouteBatches(toBeDispatchedImmediately, receiveContext);
        }
    }

}