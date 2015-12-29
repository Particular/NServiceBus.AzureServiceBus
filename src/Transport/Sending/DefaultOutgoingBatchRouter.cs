namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Logging;
    using NServiceBus.Settings;

    public class DefaultOutgoingBatchRouter : IRouteOutgoingBatches
    {
        ILog logger = LogManager.GetLogger<DefaultOutgoingBatchRouter>();
        readonly IConvertOutgoingMessagesToBrokeredMessages outgoingMessageConverter;
        readonly IManageMessageSenderLifeCycle senders;
        private readonly ReadOnlySettings settings;
        private readonly IHandleOversizedBrokeredMessages oversizedMessageHandler;

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

        public async Task RouteBatches(IEnumerable<Batch> outgoingBatches, ReceiveContext context)
        {
            foreach (var batch in outgoingBatches)
            {
                await RouteBatch(batch, context);
            }
        }

        public async Task RouteBatch(Batch batch, ReceiveContext context)
        {
            var outgoingBatches = batch.Operations;
            if (!outgoingBatches.Any()) return;

            var activeNamespaces = batch.Destinations.Namespaces.Where(n => n.Mode == NamespaceMode.Active);
            //var passiveNamespaces = batch.Destinations.Namespaces.Where(n => n.Mode == NamespaceMode.Passive);
            foreach (var entity in batch.Destinations.Entities)
            {
                var routingOptions = GetRoutingOptions(context);

                if (!string.IsNullOrEmpty(routingOptions.ViaEntityPath))
                {
                    Logger.InfoFormat("Routing {0} messages to {1} via {2}", outgoingBatches.Count, entity.Path, routingOptions.ViaEntityPath);
                }
                else
                {
                    Logger.InfoFormat("Routing {0} messages to {1}", outgoingBatches.Count, entity.Path);
                }

                var pendingSends = new List<Task>();
                // TODO: ensure fallback to passives
                foreach (var ns in activeNamespaces)
                {
                    var messageSender = senders.Get(entity.Path, routingOptions.ViaEntityPath, ns.ConnectionString);

                    var brokeredMessages = outgoingMessageConverter.Convert(outgoingBatches, routingOptions);

                    pendingSends.Add(RouteOutBatchesAndLogExceptionsAsync(messageSender, brokeredMessages));
                }

                await Task.WhenAll(pendingSends).ConfigureAwait(false);
            }
        }

        private RoutingOptions GetRoutingOptions(ReceiveContext receiveContext)
        {
            var sendVia = settings.Get<bool>(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue);
            var context = receiveContext as BrokeredMessageReceiveContext;
            return new RoutingOptions
            {
                SendVia = sendVia,
                ViaEntityPath = GetViaEntityPathFor(context?.Entity),
                ViaConnectionString = context?.Entity.Namespace.ConnectionString,
                ViaPartitionKey = context?.IncomingBrokeredMessage.PartitionKey
            };
        }

        private string GetViaEntityPathFor(EntityInfo entity)
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

        private async Task RouteOutBatchesAndLogExceptionsAsync(IMessageSender messageSender, IEnumerable<BrokeredMessage> messagesToSend)
        {
            try
            {
                await RouteBatchWithEnforcedBatchSizeAsync(messageSender, messagesToSend);
            }
            catch (Exception exception)
            {
                // ASB team promissed to fix the issue with MessagingEntityNotFoundException (missing entity path) - verify that
                var message = "Failed to dispatch a batch with the following message IDs: " + string.Join(", ", messagesToSend.Select(x => x.MessageId));
                logger.Error(message, exception);
                throw;
            }
        }

        async Task RouteBatchWithEnforcedBatchSizeAsync(IMessageSender messageSender, IEnumerable<BrokeredMessage> messagesToSend)
        {
            var chunk = new List<BrokeredMessage>();
            long batchSize = 0;

            foreach (var message in messagesToSend)
            {
                if (GuardMessageSize(message))
                {
                    return;
                }

                if ((batchSize + message.Size) > maximuMessageSizeInKilobytes * 1024)
                {
                    if (chunk.Any())
                    {
                        var currentChunk = chunk;
                        await messageSender.RetryOnThrottleAsync(s => s.SendBatch(currentChunk), s => s.SendBatch(currentChunk.Select(x => x.Clone())), backOffTimeOnThrottle, maxRetryAttemptsOnThrottle).ConfigureAwait(false);
                    }

                    chunk = new List<BrokeredMessage> { message };
                    batchSize = message.Size;
                }
                else
                {
                    chunk.Add(message);
                    batchSize += message.Size;
                }
            }

            if (chunk.Any())
            {
                await messageSender.RetryOnThrottleAsync(s => s.SendBatch(chunk), s => s.SendBatch(chunk.Select(x => x.Clone())), backOffTimeOnThrottle, maxRetryAttemptsOnThrottle).ConfigureAwait(false);
            }
        }

        bool GuardMessageSize(BrokeredMessage brokeredMessage)
        {
            if (brokeredMessage.Size > maximuMessageSizeInKilobytes * 1024)
            {
                oversizedMessageHandler.Handle(brokeredMessage);
                return true;
            }

            return false;
        }

        static ILog Logger = LogManager.GetLogger<DefaultOutgoingBatchRouter>();
    }
}