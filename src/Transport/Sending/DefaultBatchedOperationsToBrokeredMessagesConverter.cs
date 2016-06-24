namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.ServiceBus.Messaging;
    using Topology.MetaModel;
    using DelayedDelivery;
    using Settings;
    using Transports;

    class DefaultBatchedOperationsToBrokeredMessagesConverter : IConvertOutgoingMessagesToBrokeredMessages
    {
        ReadOnlySettings settings;
        ICanMapNamespaceNameToConnectionString mapper;

        public DefaultBatchedOperationsToBrokeredMessagesConverter(ReadOnlySettings settings, ICanMapNamespaceNameToConnectionString mapper)
        {
            this.settings = settings;
            this.mapper = mapper;
        }

        public IEnumerable<BrokeredMessage> Convert(IEnumerable<BatchedOperation> outgoingMessages, RoutingOptions routingOptions)
        {
            return outgoingMessages.Select(x => Convert(x, routingOptions));
        }

        public BrokeredMessage Convert(BatchedOperation outgoingOperation, RoutingOptions routingOptions)
        {
            var outgoingMessage = outgoingOperation.Message;
            var brokeredMessage = CreateBrokeredMessage(outgoingMessage);
            brokeredMessage.MessageId = Guid.NewGuid().ToString();

            ApplyDeliveryConstraints(brokeredMessage, outgoingOperation);

            ApplyTimeToLive(outgoingMessage, brokeredMessage);

            ApplyCorrelationId(outgoingMessage, brokeredMessage);

            SetReplyToAddress(outgoingMessage, brokeredMessage);

            SetViaPartitionKeyToIncomingBrokeredMessagePartitionKey(brokeredMessage, routingOptions);

            SetEstimatedMessageSizeHeader(brokeredMessage, outgoingOperation.GetEstimatedSize());

            CopyHeaders(outgoingMessage, brokeredMessage);

            return brokeredMessage;
        }

        void SetEstimatedMessageSizeHeader(BrokeredMessage brokeredMessage, long estimatedSize)
        {
            brokeredMessage.Properties[BrokeredMessageHeaders.EstimatedMessageSize] = estimatedSize;
        }

        void SetViaPartitionKeyToIncomingBrokeredMessagePartitionKey(BrokeredMessage brokeredMessage, RoutingOptions routingOptions)
        {
            if (routingOptions.SendVia && routingOptions.ViaPartitionKey != null)
            {
                brokeredMessage.ViaPartitionKey = routingOptions.ViaPartitionKey;
            }
        }

        void SetReplyToAddress(OutgoingMessage outgoingMessage, BrokeredMessage brokeredMessage)
        {
            if (outgoingMessage.Headers.ContainsKey(Headers.ReplyToAddress))
            {
                var replyTo = mapper.Map(outgoingMessage.Headers[Headers.ReplyToAddress]);

                // ensure that the reply to address contains namespace info in case the message is sent to destination only namespace.
                if (!replyTo.HasSuffix)
                {
                    var namespaces = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
                    var defaultName = settings.Get<string>(WellKnownConfigurationKeys.Topology.Addressing.DefaultNamespaceName);
                    var selected = namespaces.FirstOrDefault(ns => ns.Name == defaultName);
                    if (selected == null)
                    {
                        selected = namespaces.FirstOrDefault(ns => ns.Purpose == NamespacePurpose.Partitioning);
                    }

                    if (selected != null)
                    {
                        replyTo = mapper.Map(new EntityAddress(replyTo.Name, selected.Name));
                    }
                }

                outgoingMessage.Headers[Headers.ReplyToAddress] = replyTo;
                brokeredMessage.ReplyTo = replyTo;
            }
        }

        void ApplyCorrelationId(OutgoingMessage outgoingMessage, BrokeredMessage brokeredMessage)
        {
            if (outgoingMessage.Headers.ContainsKey(Headers.CorrelationId))
            {
                brokeredMessage.CorrelationId = outgoingMessage.Headers[Headers.CorrelationId];
            }
        }

        void ApplyTimeToLive(OutgoingMessage outgoingMessage, BrokeredMessage brokeredMessage)
        {
            TimeSpan? timeToLive = null;
            if (outgoingMessage.Headers.ContainsKey(Headers.TimeToBeReceived))
            {
                TimeSpan ttl;
                TimeSpan.TryParse(outgoingMessage.Headers[Headers.TimeToBeReceived], out ttl);
                timeToLive = ttl;
            }

            if (timeToLive.HasValue && timeToLive.Value > TimeSpan.Zero)
            {
                brokeredMessage.TimeToLive = timeToLive.Value;
            }
        }

        void ApplyDeliveryConstraints(BrokeredMessage brokeredMessage, BatchedOperation operation)
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
        }

        static void CopyHeaders(OutgoingMessage outgoingMessage, BrokeredMessage brokeredMessage)
        {
            foreach (var header in outgoingMessage.Headers)
            {
                brokeredMessage.Properties[header.Key] = header.Value;
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