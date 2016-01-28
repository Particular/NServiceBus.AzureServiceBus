namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.Transports;

    public interface IBatcher
    {
        IList<Batch> ToBatches(TransportOperations operations);
    }

    class Batcher : IBatcher
    {
        private readonly ITopologySectionManager topologySectionManager;
        
        public Batcher(ITopologySectionManager topologySectionManager)
        {
            this.topologySectionManager = topologySectionManager;
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
                batch.Operations.Add(new BatchedOperation
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
                batch.Operations.Add(new BatchedOperation
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

    public class Batch
    {
        public Batch()
        {
            Operations = new List<BatchedOperation>();
        }

        public TopologySection Destinations { get; set; }

        public DispatchConsistency RequiredDispatchConsistency { get; set; }

        public IList<BatchedOperation> Operations { get; set; }
    }

    public class BatchedOperation
    {
        public OutgoingMessage Message { get; set; }

        public IEnumerable<DeliveryConstraint> DeliveryConstraints { get; set; }

        public long GetEstimatedSize()
        {
            const int assumeSize = 256;
            var standardPropertiesSize = GetStringSizeInBytes(Message.MessageId) +
                                         assumeSize + // ContentType +
                                         assumeSize + // CorrelationId +
                                         4 + // DeliveryCount
                                         8 + // EnqueuedSequenceNumber
                                         8 + // EnqueuedTimeUtc
                                         8 + // ExpiresAtUtc
                                         1 + // ForcePersistence
                                         1 + // IsBodyConsumed
                                         assumeSize + // Label
                                         8 + // LockedUntilUtc 
                                         16 + // LockToken 
                                         assumeSize + // PartitionKey
                                         8 + // ScheduledEnqueueTimeUtc
                                         8 + // SequenceNumber
                                         assumeSize + // SessionId
                                         4 + // State
                                         8 + // TimeToLive +
                                         assumeSize + // To +
                                         assumeSize;  // ViaPartitionKey;

            var headers = Message.Headers.Sum(property => GetStringSizeInBytes(property.Key) + GetStringSizeInBytes(property.Value));
            var bodySize = Message.Body.Length;
            var total = standardPropertiesSize + headers + bodySize;

            const double addTenPercent = 1.1;
            var estimatedSize = (long)(total * addTenPercent);
            return estimatedSize;
        }

        private static int GetStringSizeInBytes(string value) => value != null ? Encoding.UTF8.GetByteCount(value) : 0;
    }
}