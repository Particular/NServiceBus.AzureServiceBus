namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Settings;
    using Transport;

    class Batcher : IBatcherInternal
    {
        ITopologySectionManagerInternal topologySectionManager;
        int messageSizePaddingPercentage;

        public Batcher(ITopologySectionManagerInternal topologySectionManager, ReadOnlySettings settings)
        {
            this.topologySectionManager = topologySectionManager;
            messageSizePaddingPercentage = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.MessageSizePaddingPercentage);
        }

        public async Task<List<BatchInternal>> ToBatches(TransportOperations operations)
        {
            var indexedBatches = new Dictionary<string, BatchInternal>();
            await AddMulticastOperationBatches(operations, indexedBatches).ConfigureAwait(false);
            AddUnicastOperationBatches(operations, indexedBatches);
            return indexedBatches.Values.ToList();
        }

        void AddUnicastOperationBatches(TransportOperations operations, Dictionary<string, BatchInternal> indexedBatches)
        {
            foreach (var unicastOperation in operations.UnicastTransportOperations)
            {
                var key = $"unicast-{unicastOperation.Destination}-consistency-{unicastOperation.RequiredDispatchConsistency}";
                BatchInternal batch;
                if (!indexedBatches.TryGetValue(key, out batch))
                {
                    batch = new BatchInternal();
                    indexedBatches[key] = batch;

                    batch.Destinations = topologySectionManager.DetermineSendDestination(unicastOperation.Destination);
                    batch.RequiredDispatchConsistency = unicastOperation.RequiredDispatchConsistency;
                }
                batch.Operations.Add(new BatchedOperationInternal(messageSizePaddingPercentage)
                {
                    Message = unicastOperation.Message,
                    DeliveryConstraints = unicastOperation.DeliveryConstraints,
                });
            }
        }

        async Task AddMulticastOperationBatches(TransportOperations operations, Dictionary<string, BatchInternal> indexedBatches)
        {
            foreach (var multicastOperation in operations.MulticastTransportOperations)
            {
                var key = $"multicast-{multicastOperation.MessageType}-consistency-{multicastOperation.RequiredDispatchConsistency}";
                BatchInternal batch;
                if (!indexedBatches.TryGetValue(key, out batch))
                {
                    batch = new BatchInternal();
                    indexedBatches[key] = batch;

                    batch.Destinations = await topologySectionManager.DeterminePublishDestination(multicastOperation.MessageType).ConfigureAwait(false);
                    batch.RequiredDispatchConsistency = multicastOperation.RequiredDispatchConsistency;
                }
                batch.Operations.Add(new BatchedOperationInternal(messageSizePaddingPercentage)
                {
                    DeliveryConstraints = multicastOperation.DeliveryConstraints,
                    Message = multicastOperation.Message
                });
            }
        }
    }
}