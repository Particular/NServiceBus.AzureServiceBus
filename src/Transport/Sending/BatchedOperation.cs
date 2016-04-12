namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DeliveryConstraints;
    using Transports;

    public class BatchedOperation
    {
        int messageSizePaddingPercentage;

        public BatchedOperation(int messageSizePaddingPercentage = 0)
        {
            this.messageSizePaddingPercentage = messageSizePaddingPercentage;
        }

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

            var padWithPercentage = (double)(100 + messageSizePaddingPercentage) / 100;
            var estimatedSize = (long)(total * padWithPercentage);
            return estimatedSize;
        }

        static int GetStringSizeInBytes(string value) => value != null ? Encoding.UTF8.GetByteCount(value) : 0;
    }
}