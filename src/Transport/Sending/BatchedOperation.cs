namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DeliveryConstraints;

    class BatchedOperationInternal
    {
        public BatchedOperationInternal(int messageSizePaddingPercentage = 0)
        {
            this.messageSizePaddingPercentage = messageSizePaddingPercentage;
        }

        public OutgoingMessage Message { get; set; }

        public IEnumerable<DeliveryConstraint> DeliveryConstraints { get; set; }

        public long GetEstimatedSize()
        {
            const int stringHeaderValueAssumedSizeInBytes = 256;
            var standardPropertiesSize = GetStringSizeInBytes(Message.MessageId) +
                                         stringHeaderValueAssumedSizeInBytes + // ContentType +
                                         stringHeaderValueAssumedSizeInBytes + // CorrelationId +
                                         4 + // DeliveryCount
                                         8 + // EnqueuedSequenceNumber
                                         8 + // EnqueuedTimeUtc
                                         8 + // ExpiresAtUtc
                                         1 + // ForcePersistence
                                         1 + // IsBodyConsumed
                                         stringHeaderValueAssumedSizeInBytes + // Label
                                         8 + // LockedUntilUtc
                                         16 + // LockToken
                                         stringHeaderValueAssumedSizeInBytes + // PartitionKey
                                         8 + // ScheduledEnqueueTimeUtc
                                         8 + // SequenceNumber
                                         stringHeaderValueAssumedSizeInBytes + // SessionId
                                         4 + // State
                                         8 + // TimeToLive +
                                         stringHeaderValueAssumedSizeInBytes + // To +
                                         stringHeaderValueAssumedSizeInBytes; // ViaPartitionKey;

            var headers = Message.Headers.Sum(property => GetStringSizeInBytes(property.Key) + GetStringSizeInBytes(property.Value));
            var bodySize = Message.Body.Length;
            var total = standardPropertiesSize + headers + bodySize;

            var padWithPercentage = (double)(100 + messageSizePaddingPercentage) / 100;
            var estimatedSize = (long)(total * padWithPercentage);
            return estimatedSize;
        }

        static int GetStringSizeInBytes(string value) => value != null ? Encoding.UTF8.GetByteCount(value) : 0;
        int messageSizePaddingPercentage;
    }
}