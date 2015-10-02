namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    public class DefaultOutgoingMessageRouter : IRouteOutgoingMessages
    {
        readonly IAddressingStrategy addressingStrategy;
        readonly IConvertOutgoingMessagesToBrokeredMessages outgoingMessageConverter;
        readonly IManageClientEntityLifeCycle senders;

        int maxRetryAttemptsOnThrottle;
        TimeSpan backOffTimeOnThrottle;
        int maximuMessageSizeInKilobytes;

        public DefaultOutgoingMessageRouter(IAddressingStrategy addressingStrategy, IConvertOutgoingMessagesToBrokeredMessages outgoingMessageConverter, IManageClientEntityLifeCycle senders, ReadOnlySettings settings)
        {
            this.addressingStrategy = addressingStrategy;
            this.outgoingMessageConverter = outgoingMessageConverter;
            this.senders = senders;

            backOffTimeOnThrottle = settings.Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessageSenders.BackOffTimeOnThrottle);
            maxRetryAttemptsOnThrottle = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryAttemptsOnThrottle);
            maximuMessageSizeInKilobytes = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximuMessageSizeInKilobytes);
        }

        public async Task RouteAsync(OutgoingMessage message, DispatchOptions dispatchOptions)
        {
            var address = GetAddress(dispatchOptions);

            var messageSender = (IMessageSender)senders.Get(address.Path, address.Namespace.ConnectionString);

            var brokeredMessage = outgoingMessageConverter.Convert(message, dispatchOptions);
            await messageSender.RetryOnThrottle(s => s.SendAsync(brokeredMessage), backOffTimeOnThrottle, maxRetryAttemptsOnThrottle);
        }

        public async Task RouteBatchAsync(IEnumerable<OutgoingMessage> messages, DispatchOptions dispatchOptions)
        {
            var address = GetAddress(dispatchOptions);

            var messageSender = (IMessageSender)senders.Get(address.Path, address.Namespace.ConnectionString);

            var brokeredMessages = outgoingMessageConverter.Convert(messages, dispatchOptions);

            await SendBatchWithEnforcedBatchSize(messageSender, brokeredMessages);
        }

        EntityInfo GetAddress(DispatchOptions dispatchOptions)
        {
            var directRouting = dispatchOptions.RoutingStrategy as DirectToTargetDestination;

            if (directRouting == null) // publish
            {
                var toAllSubscribers = (ToAllSubscribers)dispatchOptions.RoutingStrategy;

                return addressingStrategy.GetEntitiesForPublishing(toAllSubscribers.EventType).FirstOrDefault();
            }
            else // send
            {
                return addressingStrategy.GetEntitiesForSending(directRouting.Destination).FirstOrDefault();
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