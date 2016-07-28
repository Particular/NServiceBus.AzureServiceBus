namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Azure.Transports.WindowsAzureServiceBus;
    using Logging;
    using Settings;

    class DefaultOutgoingBatchRouter : IRouteOutgoingBatches
    {
        ILog logger = LogManager.GetLogger<DefaultOutgoingBatchRouter>();
        IConvertOutgoingMessagesToBrokeredMessages outgoingMessageConverter;
        IManageMessageSenderLifeCycle senders;
        ReadOnlySettings settings;
        IHandleOversizedBrokeredMessages oversizedMessageHandler;

        int maxRetryAttemptsOnThrottle;
        TimeSpan backOffTimeOnThrottle;
        int maximuMessageSizeInKilobytes;

        public DefaultOutgoingBatchRouter(IConvertOutgoingMessagesToBrokeredMessages outgoingMessageConverter, IManageMessageSenderLifeCycle senders, ReadOnlySettings settings, IHandleOversizedBrokeredMessages oversizedMessageHandler)
        {
            this.outgoingMessageConverter = outgoingMessageConverter;
            this.senders = senders;
            this.settings = settings;
            this.oversizedMessageHandler = oversizedMessageHandler;

            backOffTimeOnThrottle = settings.Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessageSenders.BackOffTimeOnThrottle);
            maxRetryAttemptsOnThrottle = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryAttemptsOnThrottle);
            maximuMessageSizeInKilobytes = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximumMessageSizeInKilobytes);
        }

        public Task RouteBatches(IEnumerable<Batch> outgoingBatches, ReceiveContext context)
        {
            var pendingBatches = new List<Task>();
            foreach (var batch in outgoingBatches)
            {
                pendingBatches.Add(RouteBatch(batch, context));
            }
            return Task.WhenAll(pendingBatches);
        }

        public Task RouteBatch(Batch batch, ReceiveContext context)
        {
            var outgoingBatches = batch.Operations;

            var activeNamespaces = batch.Destinations.Namespaces.Where(n => n.Mode == NamespaceMode.Active).ToList();
            var passiveNamespaces = batch.Destinations.Namespaces.Where(n => n.Mode == NamespaceMode.Passive).ToList();
            var pendingSends = new List<Task>();

            foreach (var entity in batch.Destinations.Entities)
            {
                var routingOptions = GetRoutingOptions(context);

                if (!string.IsNullOrEmpty(routingOptions.ViaEntityPath))
                {
                    Logger.DebugFormat("Routing {0} messages to {1} via {2}", outgoingBatches.Count, entity.Path, routingOptions.ViaEntityPath);
                }
                else
                {
                    Logger.DebugFormat("Routing {0} messages to {1}", outgoingBatches.Count, entity.Path);
                }

                // don't use via on fallback, not supported across namespaces
                var fallbacks = passiveNamespaces.Select(ns => senders.Get(entity.Path, null, ns.Name)).ToList();

                foreach (var ns in activeNamespaces)
                {
                    // only use via if the destination and via namespace are the same
                    var via = ns.ConnectionString == routingOptions.ViaConnectionString ? routingOptions.ViaEntityPath : null;
                    var messageSender = senders.Get(entity.Path, via, ns.Name);

                    var brokeredMessages = outgoingMessageConverter.Convert(outgoingBatches, routingOptions).ToList();

                    pendingSends.Add(RouteOutBatchesWithFallbackAndLogExceptionsAsync(messageSender, fallbacks, brokeredMessages));
                }
            }
            return Task.WhenAll(pendingSends);
        }

        RoutingOptions GetRoutingOptions(ReceiveContext receiveContext)
        {
            var sendVia = settings.Get<bool>(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue);
            var context = receiveContext as BrokeredMessageReceiveContext;
            if (context?.Recovering == true) // avoid send via when recovering
            {
                sendVia = false;
            }
            return new RoutingOptions
            {
                SendVia = sendVia,
                ViaEntityPath = GetViaEntityPathFor(context?.Entity),
                ViaConnectionString = context?.Entity.Namespace.ConnectionString,
                ViaPartitionKey = context?.IncomingBrokeredMessage.PartitionKey
            };
        }

        string GetViaEntityPathFor(EntityInfo entity)
        {
            if (entity?.Type == EntityType.Queue)
            {
                return entity.Path;
            }
            if (entity?.Type == EntityType.Subscription)
            {
                var topicRelationship = entity.RelationShips.First(r => r.Type == EntityRelationShipType.Subscription);
                return topicRelationship.Target.Path;
            }

            return null;
        }

        async Task RouteOutBatchesWithFallbackAndLogExceptionsAsync(IMessageSender messageSender, IList<IMessageSender> fallbacks, IList<BrokeredMessage> messagesToSend)
        {
            try
            {
                await RouteBatchWithEnforcedBatchSizeAsync(messageSender, messagesToSend).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                // ASB team promissed to fix the issue with MessagingEntityNotFoundException (missing entity path) - verify that
                var message = "Failed to dispatch a batch with the following message IDs: " + string.Join(", ", messagesToSend.Select(x => x.MessageId));
                logger.Error(message, exception);

                // no need to try and send too large messages to another namespace, won't work
                if (exception is MessageTooLargeException)
                    throw;

                var fallBackSucceeded = false;
                if (fallbacks.Any())
                {
                    foreach (var fallback in fallbacks)
                    {
                        var clones = messagesToSend.Select(x => x.Clone()).ToList();
                        try
                        {
                            await RouteBatchWithEnforcedBatchSizeAsync(fallback, clones).ConfigureAwait(false);
                            logger.Info("Successfully dispatched a batch with the following message IDs: " + string.Join(", ", clones.Select(x => x.MessageId) + " to fallback namespace"));
                            fallBackSucceeded = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Failed to dispatch batch to fallback namespace: ${ex}");
                        }
                    }
                }

                if(!fallBackSucceeded) throw;
            }
        }

        async Task RouteBatchWithEnforcedBatchSizeAsync(IMessageSender messageSender, IEnumerable<BrokeredMessage> messagesToSend)
        {
            var chunk = new List<BrokeredMessage>();
            long batchSize = 0;
            var chunkNumber = 1;

            foreach (var message in messagesToSend)
            {
                if (GuardMessageSize(message))
                {
                    return;
                }

                var messageSize = message.EstimatedSize();

                if (batchSize + messageSize > maximuMessageSizeInKilobytes * 1024)
                {
                    if (chunk.Any())
                    {
                        logger.Debug($"Routing batched messages, chunk #{chunkNumber++}.");
                        var currentChunk = chunk;
                        await messageSender.RetryOnThrottleAsync(s => s.SendBatch(currentChunk), s => s.SendBatch(currentChunk.Select(x => x.Clone())), backOffTimeOnThrottle, maxRetryAttemptsOnThrottle).ConfigureAwait(false);
                    }

                    chunk = new List<BrokeredMessage> { message };
                    batchSize = messageSize;
                }
                else
                {
                    chunk.Add(message);
                    batchSize += messageSize;
                }
            }

            if (chunk.Any())
            {
                logger.Debug($"Routing batched messages, chunk #{chunkNumber}.");
                await messageSender.RetryOnThrottleAsync(s => s.SendBatch(chunk), s => s.SendBatch(chunk.Select(x => x.Clone())), backOffTimeOnThrottle, maxRetryAttemptsOnThrottle).ConfigureAwait(false);
            }
        }

        bool GuardMessageSize(BrokeredMessage brokeredMessage)
        {
            var estimatedSize = brokeredMessage.EstimatedSize();
            if (estimatedSize > maximuMessageSizeInKilobytes * 1024)
            {
                logger.Debug($"Detected an outgoing message that exceeds the maximum message size allowed by Azure ServiceBus. Estimated message size is {estimatedSize} bytes.");
                oversizedMessageHandler.Handle(brokeredMessage);
                return true;
            }

            return false;
        }

        static ILog Logger = LogManager.GetLogger<DefaultOutgoingBatchRouter>();
    }
}