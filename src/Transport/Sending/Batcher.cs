namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    class Batcher : IBatcher
    {
        private readonly ITopologySectionManager topologySectionManager;
        private int messageSizePaddingPercentage;

        public Batcher(ITopologySectionManager topologySectionManager, ReadOnlySettings settings)
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

        private void AddUnicastOperationBatches(TransportOperations operations, Dictionary<string, Batch> indexedBatches)
        {
            foreach (var unicastOperation in operations.UnicastTransportOperations)
            {
                var key = ComputeKeyFor(unicastOperation);
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

        private void AddMulticastOperationBatches(TransportOperations operations, Dictionary<string, Batch> indexedBatches)
        {
            foreach (var multicastOperation in operations.MulticastTransportOperations)
            {
                var key = ComputeKeyFor(multicastOperation);
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

        string ComputeKeyFor(MulticastTransportOperation operation)
        {
            var sb = new StringBuilder();
            sb.Append($"multicast;-{operation.MessageType}");
            sb.Append($"-consistency-{operation.RequiredDispatchConsistency}");
            return sb.ToString();
        }

        string ComputeKeyFor(UnicastTransportOperation operation)
        {
            var sb = new StringBuilder();

            sb.Append($"unicast-{operation.Destination}");
            sb.Append($"-consistency-{operation.RequiredDispatchConsistency}");

            return sb.ToString();
        }
    }
}