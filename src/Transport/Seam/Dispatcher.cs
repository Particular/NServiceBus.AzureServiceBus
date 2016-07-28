namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;
    using Extensibility;
    using Microsoft.ServiceBus.Messaging;
    using Settings;
    using Transports;

    class Dispatcher : IDispatchMessages
    {
        public Dispatcher(ReadOnlySettings settings, IRouteOutgoingBatches routeOutgoingBatches, IBatcher batcher)
        {
            this.settings = settings;
            this.routeOutgoingBatches = routeOutgoingBatches;
            this.batcher = batcher;
        }

        public Task Dispatch(TransportOperations operations, ContextBag context)
        {
            var outgoingBatches = batcher.ToBatches(operations);

            ReceiveContext receiveContext;
            if (!TryGetReceiveContext(context, out receiveContext)) // not in a receive context, so send out immediately
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

            return DispatchAccordingToIsolationLevel(outgoingBatches, receiveContext);
        }

        async Task DispatchAccordingToIsolationLevel(IList<Batch> outgoingBatches, BrokeredMessageReceiveContext receiveContext)
        {
            var batchesWithIsolatedDispatchConsistency = outgoingBatches.Where(t => t.RequiredDispatchConsistency == DispatchConsistency.Isolated);
            var batchesWithDefaultConsistency = outgoingBatches.Where(t => t.RequiredDispatchConsistency == DispatchConsistency.Default).ToList();

            await routeOutgoingBatches.RouteBatches(batchesWithIsolatedDispatchConsistency, receiveContext).ConfigureAwait(false);
            await DispatchWithTransactionScopeIfRequired(batchesWithDefaultConsistency, receiveContext).ConfigureAwait(false);
        }

        async Task DispatchWithTransactionScopeIfRequired(IList<Batch> toBeDispatchedOnComplete, BrokeredMessageReceiveContext context)
        {
            if (context.CancellationToken.IsCancellationRequested || !toBeDispatchedOnComplete.Any())
                return;

            // send via receive queue only works when wrapped in a scope
            var useTx = settings.Get<bool>(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue);
            using (var scope = useTx ? new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled) : null)
            {
                await routeOutgoingBatches.RouteBatches(toBeDispatchedOnComplete, context).ConfigureAwait(false);
                scope?.Complete();
            }
        }

        static bool TryGetReceiveContext(ContextBag context, out ReceiveContext receiveContext)
        {
            TransportTransaction transportTransaction;

            if (!context.TryGet(out transportTransaction))
            {
                receiveContext = null;
                return false;
            }

            return transportTransaction.TryGet(out receiveContext);
        }

        IRouteOutgoingBatches routeOutgoingBatches;
        IBatcher batcher;
        ReadOnlySettings settings;
    }
}