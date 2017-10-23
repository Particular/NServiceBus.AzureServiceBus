namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;

    class Batcher : IBatcherInternal
    {
        public Batcher(ITopologySectionManagerInternal topologySectionManager, int messageSizePaddingPercentage, string localAddress)
        {
            this.topologySectionManager = topologySectionManager;
            this.messageSizePaddingPercentage = messageSizePaddingPercentage;
            this.localAddress = localAddress;
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
                if (!indexedBatches.TryGetValue(key, out var batch))
                {
                    batch = new BatchInternal();
                    indexedBatches[key] = batch;

                    batch.Destinations = topologySectionManager.DetermineSendDestination(unicastOperation.Destination);
                    batch.RequiredDispatchConsistency = unicastOperation.RequiredDispatchConsistency;
                }
                batch.Operations.Add(new BatchedOperationInternal(messageSizePaddingPercentage)
                {
                    Message = unicastOperation.Message,
                    DeliveryConstraints = unicastOperation.DeliveryConstraints
                });
            }
        }

        void AddMulticastOperationBatches(TransportOperations operations, Dictionary<string, BatchInternal> indexedBatches)
        {
            foreach (var multicastOperation in operations.MulticastTransportOperations)
            {
                var key = $"multicast-{multicastOperation.MessageType}-consistency-{multicastOperation.RequiredDispatchConsistency}";
                if (!indexedBatches.TryGetValue(key, out var batch))
                {
                    batch = new BatchInternal();
                    indexedBatches[key] = batch;

                    batch.Destinations = topologySectionManager.DeterminePublishDestination(multicastOperation.MessageType, localAddress);
                    batch.RequiredDispatchConsistency = multicastOperation.RequiredDispatchConsistency;
                }
                batch.Operations.Add(new BatchedOperationInternal(messageSizePaddingPercentage)
                {
                    DeliveryConstraints = multicastOperation.DeliveryConstraints,
                    Message = multicastOperation.Message
                });
            }
        }

        ITopologySectionManagerInternal topologySectionManager;
        int messageSizePaddingPercentage;
        string localAddress;
    }
}