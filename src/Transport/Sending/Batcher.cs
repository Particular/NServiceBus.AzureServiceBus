namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using Settings;
    using Transport;

    class Batcher : IBatcher
    {
        ITopologySectionManagerInternal topologySectionManager;
        int messageSizePaddingPercentage;

        public Batcher(ITopologySectionManagerInternal topologySectionManager, ReadOnlySettings settings)
        {
            this.topologySectionManager = topologySectionManager;
            messageSizePaddingPercentage = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.MessageSizePaddingPercentage);
        }

        public IList<Batch> ToBatches(TransportOperations operations)
        {
            var indexedBatches = new Dictionary<string, Batch>();
            AddMulticastOperationBatches(operations, indexedBatches);
            AddUnicastOperationBatches(operations, indexedBatches);
            return indexedBatches.Values.ToList();
        }

        void AddUnicastOperationBatches(TransportOperations operations, Dictionary<string, Batch> indexedBatches)
        {
            foreach (var unicastOperation in operations.UnicastTransportOperations)
            {
                var key = $"unicast-{unicastOperation.Destination}-consistency-{unicastOperation.RequiredDispatchConsistency}";
                Batch batch;
                if (!indexedBatches.TryGetValue(key, out batch))
                {
                    batch = new Batch();
                    indexedBatches[key] = batch;

                    batch.Destinations = topologySectionManager.DetermineSendDestination(unicastOperation.Destination);
                    batch.RequiredDispatchConsistency = unicastOperation.RequiredDispatchConsistency;
                }
                batch.Operations.Add(new BatchedOperation(messageSizePaddingPercentage)
                {
                    Message = unicastOperation.Message,
                    DeliveryConstraints = unicastOperation.DeliveryConstraints,
                });
            }
        }

        void AddMulticastOperationBatches(TransportOperations operations, Dictionary<string, Batch> indexedBatches)
        {
            foreach (var multicastOperation in operations.MulticastTransportOperations)
            {
                var key = $"multicast-{multicastOperation.MessageType}-consistency-{multicastOperation.RequiredDispatchConsistency}";
                Batch batch;
                if (!indexedBatches.TryGetValue(key, out batch))
                {
                    batch = new Batch();
                    indexedBatches[key] = batch;

                    batch.Destinations = topologySectionManager.DeterminePublishDestination(multicastOperation.MessageType);
                    batch.RequiredDispatchConsistency = multicastOperation.RequiredDispatchConsistency;
                }
                batch.Operations.Add(new BatchedOperation(messageSizePaddingPercentage)
                {
                    DeliveryConstraints = multicastOperation.DeliveryConstraints,
                    Message = multicastOperation.Message
                });
            }
        }
    }
}