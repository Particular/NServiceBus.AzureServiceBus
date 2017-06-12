namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
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

        public IList<BatchInternal> ToBatches(TransportOperations operations)
        {
            var indexedBatches = new Dictionary<string, BatchInternal>();
            AddMulticastOperationBatches(operations, indexedBatches);
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

        void AddMulticastOperationBatches(TransportOperations operations, Dictionary<string, BatchInternal> indexedBatches)
        {
            foreach (var multicastOperation in operations.MulticastTransportOperations)
            {
                var key = $"multicast-{multicastOperation.MessageType}-consistency-{multicastOperation.RequiredDispatchConsistency}";
                BatchInternal batch;
                if (!indexedBatches.TryGetValue(key, out batch))
                {
                    batch = new BatchInternal();
                    indexedBatches[key] = batch;

                    batch.Destinations = topologySectionManager.DeterminePublishDestination(multicastOperation.MessageType);
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