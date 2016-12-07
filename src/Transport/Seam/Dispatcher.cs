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
        public Dispatcher(IRouteOutgoingBatchesInternal routeOutgoingBatches, IBatcherInternal batcher)
        {
            this.routeOutgoingBatches = routeOutgoingBatches;
            this.batcher = batcher;
        }

        public Task Dispatch(TransportOperations operations, TransportTransaction transportTransaction, ContextBag context)
        {
            var outgoingBatches = batcher.ToBatches(operations);

            ReceiveContextInternal receiveContext;
            if (!TryGetReceiveContext(transportTransaction, out receiveContext)) // not in a receive context, so send out immediately
            {
                return routeOutgoingBatches.RouteBatches(outgoingBatches, null, DispatchConsistency.Default);
            }

            var brokeredMessageReceiveContext = receiveContext as BrokeredMessageReceiveContextInternal;

            if (brokeredMessageReceiveContext != null) // apply brokered message specific dispatching rules
            {
                return DispatchBatches(outgoingBatches, brokeredMessageReceiveContext);
            }
            // case when the receive context is different from brokered messaging (like eventhub)

            return routeOutgoingBatches.RouteBatches(outgoingBatches, receiveContext, DispatchConsistency.Default); // otherwise send out immediately
        }

        Task DispatchBatches(IList<BatchInternal> outgoingBatches, BrokeredMessageReceiveContextInternal receiveContext)
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

        async Task DispatchAccordingToIsolationLevel(IList<BatchInternal> outgoingBatches, BrokeredMessageReceiveContextInternal receiveContext)
        {
            var batchesWithIsolatedDispatchConsistency = outgoingBatches.Where(t => t.RequiredDispatchConsistency == DispatchConsistency.Isolated);
            var batchesWithDefaultConsistency = outgoingBatches.Where(t => t.RequiredDispatchConsistency == DispatchConsistency.Default).ToList();

            await routeOutgoingBatches.RouteBatches(batchesWithIsolatedDispatchConsistency, receiveContext, DispatchConsistency.Isolated).ConfigureAwait(false);
            await DispatchWithTransactionScopeIfRequired(batchesWithDefaultConsistency, receiveContext, DispatchConsistency.Default).ConfigureAwait(false);
        }

        async Task DispatchWithTransactionScopeIfRequired(IList<BatchInternal> toBeDispatchedOnComplete, BrokeredMessageReceiveContextInternal context, DispatchConsistency consistency)
        {
            if (context.CancellationToken.IsCancellationRequested || !toBeDispatchedOnComplete.Any())
                return;

            await routeOutgoingBatches.RouteBatches(toBeDispatchedOnComplete, context, consistency).ConfigureAwait(false);


        }

        static bool TryGetReceiveContext(TransportTransaction transportTransaction, out ReceiveContextInternal receiveContext)
        {

            if (transportTransaction == null)
            {
                receiveContext = null;
                return false;
            }

            return transportTransaction.TryGet(out receiveContext);
        }

        IRouteOutgoingBatchesInternal routeOutgoingBatches;
        IBatcherInternal batcher;
    }
}