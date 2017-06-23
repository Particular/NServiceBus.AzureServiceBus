namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.ServiceBus.Messaging;
    using DelayedDelivery;
    using Performance.TimeToBeReceived;
    using Settings;
    using Transport;

    class BatchedOperationsToBrokeredMessagesConverter
    {
        ReadOnlySettings settings;

        public BatchedOperationsToBrokeredMessagesConverter(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public IEnumerable<BrokeredMessage> Convert(IEnumerable<BatchedOperationInternal> outgoingMessages, RoutingOptionsInternal routingOptions)
        {
            return outgoingMessages.Select(x => Convert(x, routingOptions));
        }

        internal BrokeredMessage Convert(BatchedOperationInternal outgoingOperation, RoutingOptionsInternal routingOptions)
        {
            var outgoingMessage = outgoingOperation.Message;
            var brokeredMessage = CreateBrokeredMessage(outgoingMessage);
            brokeredMessage.MessageId = Guid.NewGuid().ToString();

            ApplyDeliveryConstraints(brokeredMessage, outgoingOperation);

            ApplyCorrelationId(outgoingMessage, brokeredMessage);

            SetReplyToAddress(outgoingMessage, brokeredMessage, routingOptions.DestinationNamespace);

            SetViaPartitionKeyToIncomingBrokeredMessagePartitionKey(brokeredMessage, routingOptions);

            SetEstimatedMessageSizeHeader(brokeredMessage, outgoingOperation.GetEstimatedSize());

            CopyHeaders(outgoingMessage, brokeredMessage);

            return brokeredMessage;
        }

        void SetEstimatedMessageSizeHeader(BrokeredMessage brokeredMessage, long estimatedSize)
        {
            brokeredMessage.Properties[BrokeredMessageHeaders.EstimatedMessageSize] = estimatedSize;
        }

        void SetViaPartitionKeyToIncomingBrokeredMessagePartitionKey(BrokeredMessage brokeredMessage, RoutingOptionsInternal routingOptions)
        {
            if (routingOptions.SendVia && routingOptions.ViaPartitionKey != null)
            {
                brokeredMessage.ViaPartitionKey = routingOptions.ViaPartitionKey;
            }
        }

        void SetReplyToAddress(OutgoingMessage outgoingMessage, BrokeredMessage brokeredMessage, RuntimeNamespaceInfo destinationNamespace)
        {
            if (outgoingMessage.Headers.ContainsKey(Headers.ReplyToAddress))
            {
                var replyToAddress = outgoingMessage.Headers[Headers.ReplyToAddress];
                // Read-only endpoints have no reply-to value
                if (string.IsNullOrWhiteSpace(replyToAddress))
                    return;

                var replyTo = new EntityAddress(replyToAddress);

                if (!replyTo.HasSuffix)
                {
                    var useAliases = settings.Get<bool>(WellKnownConfigurationKeys.Topology.Addressing.UseNamespaceAliasesInsteadOfConnectionStrings);

                    var selected = SelectMostAppropriateReplyToNamespace(destinationNamespace);

                    if (selected != null)
                    {
                        if (useAliases)
                        {
                            replyTo = new EntityAddress(replyTo.Name, selected.Alias);
                        }
                        else
                        {
                            replyTo = new EntityAddress(replyTo.Name, selected.Connection);
                        }
                    }
                }

                var replyToAsString = replyTo.ToString();
                brokeredMessage.ReplyTo = replyToAsString;
            }
        }

        NamespaceInfo SelectMostAppropriateReplyToNamespace(RuntimeNamespaceInfo destinationNamespace)
        {
            var namespaces = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces).Where(n => n.Purpose == NamespacePurpose.Partitioning).ToList();
            var defaultAlias = settings.Get<string>(WellKnownConfigurationKeys.Topology.Addressing.DefaultNamespaceAlias);

            var selected = destinationNamespace != null ? namespaces.FirstOrDefault(ns => ns.Alias == destinationNamespace.Alias) : null;
            if (selected == null) selected = namespaces.FirstOrDefault(ns => ns.Alias == defaultAlias);
            if (selected == null) selected = namespaces.FirstOrDefault(ns => ns.Purpose == NamespacePurpose.Partitioning);
            return selected;
        }

        void ApplyCorrelationId(OutgoingMessage outgoingMessage, BrokeredMessage brokeredMessage)
        {
            if (outgoingMessage.Headers.ContainsKey(Headers.CorrelationId))
            {
                brokeredMessage.CorrelationId = outgoingMessage.Headers[Headers.CorrelationId];
            }
        }

        void ApplyDeliveryConstraints(BrokeredMessage brokeredMessage, BatchedOperationInternal operation)
        {
            DateTime? scheduledEnqueueTime = null;

            var deliveryConstraint = operation.DeliveryConstraints.FirstOrDefault(d => d is DelayedDeliveryConstraint);

            if (deliveryConstraint != null)
            {
                var delay = deliveryConstraint as DelayDeliveryWith;
                if (delay != null)
                {
                    scheduledEnqueueTime = Time.UtcNow() + delay.Delay;
                }
                else
                {
                    var exact = deliveryConstraint as DoNotDeliverBefore;
                    if (exact != null)
                    {
                        scheduledEnqueueTime = exact.At;
                    }
                }
            }

            if (scheduledEnqueueTime.HasValue)
            {
                brokeredMessage.ScheduledEnqueueTimeUtc = scheduledEnqueueTime.Value;
            }

            var timeToBeReceivedConstraint = operation.DeliveryConstraints.FirstOrDefault(d => d is DiscardIfNotReceivedBefore) as DiscardIfNotReceivedBefore;
            if (timeToBeReceivedConstraint != null)
            {
                brokeredMessage.TimeToLive = timeToBeReceivedConstraint.MaxTime;
            }
        }

        static void CopyHeaders(OutgoingMessage outgoingMessage, BrokeredMessage brokeredMessage)
        {
            if (!string.IsNullOrEmpty(brokeredMessage.ReplyTo))
            {
                brokeredMessage.Properties[Headers.ReplyToAddress] = brokeredMessage.ReplyTo;
            }

            foreach (var header in outgoingMessage.Headers)
            {
                // BrokeredMessageHeaders.Recovery: if a message that previously failed processing is actively sent again (f.e. SLR) then the header should be removed as retry counter is reset
                // Headers.ReplyToAddress: is set by copying reply to
                if (header.Key != BrokeredMessageHeaders.Recovery && header.Key != Headers.ReplyToAddress)
                {
                    brokeredMessage.Properties[header.Key] = header.Value;
                }
            }
        }

        BrokeredMessage CreateBrokeredMessage(OutgoingMessage outgoingMessage)
        {
            BrokeredMessage brokeredMessage;
            var bodyType = settings.Get<SupportedBrokeredMessageBodyTypes>(WellKnownConfigurationKeys.Serialization.BrokeredMessageBodyType);
            switch (bodyType)
            {
                case SupportedBrokeredMessageBodyTypes.ByteArray:
                    brokeredMessage = outgoingMessage.Body != null ? new BrokeredMessage(outgoingMessage.Body) : new BrokeredMessage();
                    brokeredMessage.Properties[BrokeredMessageHeaders.TransportEncoding] = "wcf/byte-array";
                    break;

                case SupportedBrokeredMessageBodyTypes.Stream:
                    brokeredMessage = outgoingMessage.Body != null ? new BrokeredMessage(new MemoryStream(outgoingMessage.Body)) : new BrokeredMessage();
                    brokeredMessage.Properties[BrokeredMessageHeaders.TransportEncoding] = "application/octect-stream";
                    break;
                default:
                    throw new UnsupportedBrokeredMessageBodyTypeException("Unsupported brokered message body type configured");
            }
            return brokeredMessage;
        }
    }
}