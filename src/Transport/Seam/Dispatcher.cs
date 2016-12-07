namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Extensibility;
    using Microsoft.ServiceBus.Messaging;
    using Transport;

    class Dispatcher : IDispatchMessages
    {
        public Dispatcher(IRouteOutgoingBatches routeOutgoingBatches, IBatcherInternal batcher)
        {
            this.routeOutgoingBatches = routeOutgoingBatches;
            this.batcher = batcher;
        }

        public Task Dispatch(TransportOperations operations, TransportTransaction transportTransaction, ContextBag context)
        {
            var outgoingBatches = batcher.ToBatches(operations);

            ReceiveContext receiveContext;
            if (!TryGetReceiveContext(transportTransaction, out receiveContext)) // not in a receive context, so send out immediately
            {
                return routeOutgoingBatches.RouteBatches(outgoingBatches, null, DispatchConsistency.Default);
            }

            var brokeredMessageReceiveContext = receiveContext as BrokeredMessageReceiveContext;

            if (brokeredMessageReceiveContext != null) // apply brokered message specific dispatching rules
            {
                return DispatchBatches(outgoingBatches, brokeredMessageReceiveContext);
            }
            // case when the receive context is different from brokered messaging (like eventhub)

            return routeOutgoingBatches.RouteBatches(outgoingBatches, receiveContext, DispatchConsistency.Default); // otherwise send out immediately
        }

        Task DispatchBatches(IList<Batch> outgoingBatches, BrokeredMessageReceiveContext receiveContext)
        {
            // received brokered message has already been completed, so send everything out immediately
            if (receiveContext.ReceiveMode == ReceiveMode.ReceiveAndDelete)
            {
                return routeOutgoingBatches.RouteBatches(outgoingBatches, receiveContext, DispatchConsistency.Default);
            }
            // default behavior is to postpone sends until complete (and potentially complete them as a single tx if possible)
            // but some messages may need to go out immediately

            return DispatchAccordingToIsolationLevel(outgoingBatches, receiveContext);
        }

        async Task DispatchAccordingToIsolationLevel(IList<Batch> outgoingBatches, BrokeredMessageReceiveContext receiveContext)
        {
            var batchesWithIsolatedDispatchConsistency = outgoingBatches.Where(t => t.RequiredDispatchConsistency == DispatchConsistency.Isolated);
            var batchesWithDefaultConsistency = outgoingBatches.Where(t => t.RequiredDispatchConsistency == DispatchConsistency.Default).ToList();

            await routeOutgoingBatches.RouteBatches(batchesWithIsolatedDispatchConsistency, receiveContext, DispatchConsistency.Isolated).ConfigureAwait(false);
            await DispatchWithTransactionScopeIfRequired(batchesWithDefaultConsistency, receiveContext, DispatchConsistency.Default).ConfigureAwait(false);
        }

        async Task DispatchWithTransactionScopeIfRequired(IList<Batch> toBeDispatchedOnComplete, BrokeredMessageReceiveContext context, DispatchConsistency consistency)
        {
            if (context.CancellationToken.IsCancellationRequested || !toBeDispatchedOnComplete.Any())
                return;

            await routeOutgoingBatches.RouteBatches(toBeDispatchedOnComplete, context, consistency).ConfigureAwait(false);
            
            
        }

        static bool TryGetReceiveContext(TransportTransaction transportTransaction, out ReceiveContext receiveContext)
        {

            if (transportTransaction == null)
            {
                receiveContext = null;
                return false;
            }

            return transportTransaction.TryGet(out receiveContext);
        }

        IRouteOutgoingBatches routeOutgoingBatches;
        IBatcherInternal batcher;
    }
}