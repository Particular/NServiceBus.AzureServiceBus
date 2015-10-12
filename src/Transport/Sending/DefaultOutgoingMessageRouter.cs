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
        readonly ITopology topology;
        readonly IConvertOutgoingMessagesToBrokeredMessages outgoingMessageConverter;
        readonly IManageMessageSenderLifeCycle senders;

        int maxRetryAttemptsOnThrottle;
        TimeSpan backOffTimeOnThrottle;
        int maximuMessageSizeInKilobytes;

        public DefaultOutgoingMessageRouter(ITopology topology, IConvertOutgoingMessagesToBrokeredMessages outgoingMessageConverter, IManageMessageSenderLifeCycle senders, ReadOnlySettings settings)
        {
            this.topology = topology;
            this.outgoingMessageConverter = outgoingMessageConverter;
            this.senders = senders;

            backOffTimeOnThrottle = settings.Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessageSenders.BackOffTimeOnThrottle);
            maxRetryAttemptsOnThrottle = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryAttemptsOnThrottle);
            maximuMessageSizeInKilobytes = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximuMessageSizeInKilobytes);
        }

        public async Task RouteAsync(OutgoingMessage message, RoutingOptions routingOptions)
        {
            var addresses = GetAddresses(routingOptions);

            foreach (var address in addresses)
            {
                var messageSender = senders.Get(address.Path, routingOptions.ViaEntityPath, address.Namespace.ConnectionString);

                var brokeredMessage = outgoingMessageConverter.Convert(message, routingOptions.DispatchOptions);
                await messageSender.RetryOnThrottle(s => s.SendAsync(brokeredMessage), backOffTimeOnThrottle, maxRetryAttemptsOnThrottle);
            }
        }

        public async Task RouteBatchAsync(IEnumerable<OutgoingMessage> messages, RoutingOptions routingOptions)
        {
            var addresses = GetAddresses(routingOptions);
            foreach (var address in addresses)
            {
                var messageSender = senders.Get(address.Path, routingOptions.ViaEntityPath, address.Namespace.ConnectionString);

                var brokeredMessages = outgoingMessageConverter.Convert(messages, routingOptions.DispatchOptions);

                await SendBatchWithEnforcedBatchSize(messageSender, brokeredMessages);
            }
        }

        IEnumerable<EntityInfo> GetAddresses(RoutingOptions routingOptions)
        {
            var directRouting = routingOptions.DispatchOptions.RoutingStrategy as DirectToTargetDestination;

            if (directRouting == null) // publish
            {
                var toAllSubscribers = (ToAllSubscribers)routingOptions.DispatchOptions.RoutingStrategy;

                return topology.Determine(Purpose.Sending, toAllSubscribers.EventType).Entities;
            }
            else // send
            {
                return topology.Determine(Purpose.Sending, directRouting.Destination).Entities;
            }
        }

        async Task SendBatchWithEnforcedBatchSize(IMessageSender messageSender, IEnumerable<BrokeredMessage> messagesToSend)
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
                        var chunk1 = chunk;
                        await messageSender.RetryOnThrottle(s => s.SendBatchAsync(chunk1), backOffTimeOnThrottle, maxRetryAttemptsOnThrottle);
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
                await messageSender.RetryOnThrottle(s => s.SendBatchAsync(chunk), backOffTimeOnThrottle, maxRetryAttemptsOnThrottle);
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