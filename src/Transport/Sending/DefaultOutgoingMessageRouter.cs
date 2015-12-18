namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    public class DefaultOutgoingMessageRouter : IRouteOutgoingMessages
    {
        readonly ITopologySectionManager topologySectionManager;
        readonly IConvertOutgoingMessagesToBrokeredMessages outgoingMessageConverter;
        readonly IManageMessageSenderLifeCycle senders;

        int maxRetryAttemptsOnThrottle;
        TimeSpan backOffTimeOnThrottle;
        int maximuMessageSizeInKilobytes;

        public DefaultOutgoingMessageRouter(ITopologySectionManager topologySectionManager, IConvertOutgoingMessagesToBrokeredMessages outgoingMessageConverter, IManageMessageSenderLifeCycle senders, ReadOnlySettings settings)
        {
            this.topologySectionManager = topologySectionManager;
            this.outgoingMessageConverter = outgoingMessageConverter;
            this.senders = senders;

            backOffTimeOnThrottle = settings.Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessageSenders.BackOffTimeOnThrottle);
            maxRetryAttemptsOnThrottle = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryAttemptsOnThrottle);
            maximuMessageSizeInKilobytes = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximuMessageSizeInKilobytes);
        }

        public async Task RouteBatch(IEnumerable<Tuple<OutgoingMessage, DispatchOptions>> messages, RoutingOptions routingOptions)
        {
            var outgoingMessages = messages as IList<Tuple<OutgoingMessage, DispatchOptions>> ?? messages.ToList();
            if (!outgoingMessages.Any()) return;

            var addresses = GetAddresses(outgoingMessages.First().Item2); //batches are assumed grouped by address, done by the batcher
            foreach (var address in addresses)
            {
                var messageSender = senders.Get(address.Path, routingOptions.ViaEntityPath, address.Namespace.ConnectionString);

                var brokeredMessages = outgoingMessageConverter.Convert(outgoingMessages, routingOptions);

                await SendBatchWithEnforcedBatchSizeAsync(messageSender, brokeredMessages).ConfigureAwait(false); 
            }
        }

        IEnumerable<EntityInfo> GetAddresses(DispatchOptions dispatchOptions)
        {
            var directRouting = dispatchOptions.AddressTag as UnicastAddressTag;

            if (directRouting == null) // publish
            {
                var toAllSubscribers = (MulticastAddressTag)dispatchOptions.AddressTag;

                return topologySectionManager.DeterminePublishDestination(toAllSubscribers.MessageType).Entities;
            }
            else // send
            {
                return topologySectionManager.DetermineSendDestination(directRouting.Destination).Entities;
            }
        }

        async Task SendBatchWithEnforcedBatchSizeAsync(IMessageSender messageSender, IEnumerable<BrokeredMessage> messagesToSend)
        {
            var chunk = new List<BrokeredMessage>();
            long batchSize = 0;

            foreach (var message in messagesToSend)
            {
                GuardMessageSize(message);

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

        void GuardMessageSize(BrokeredMessage brokeredMessage)
        {
            if (brokeredMessage.Size > maximuMessageSizeInKilobytes * 1024)
            {
                throw new MessageTooLargeException($"The message with id {brokeredMessage.MessageId} is larger that the maximum message size allowed by Azure ServiceBus, consider using the databus feature.");
            }
        }
    }
}