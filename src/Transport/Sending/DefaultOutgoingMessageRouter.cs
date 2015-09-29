namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Routing;
    using NServiceBus.Transports;

    public class DefaultOutgoingMessageRouter : IRouteOutgoingMessages
    {
        readonly IAddressingStrategy addressingStrategy;
        readonly IConvertOutgoingMessagesToBrokeredMessages outgoingMessageConverter;
        readonly IManageClientEntityLifeCycle senders;

        public DefaultOutgoingMessageRouter(IAddressingStrategy addressingStrategy, IConvertOutgoingMessagesToBrokeredMessages outgoingMessageConverter, IManageClientEntityLifeCycle senders)
        {
            this.addressingStrategy = addressingStrategy;
            this.outgoingMessageConverter = outgoingMessageConverter;
            this.senders = senders;
        }

        public async Task RouteAsync(OutgoingMessage message, DispatchOptions dispatchOptions)
        {
            var address = GetAddress(dispatchOptions);

            var messageSender = (IMessageSender) senders.Get(address.Path, address.Namespace.ConnectionString);

            var brokeredMessage = outgoingMessageConverter.Convert(message, dispatchOptions);
            await messageSender.SendAsync(brokeredMessage);
        }

        public async Task RouteBatchAsync(IEnumerable<OutgoingMessage> messages, DispatchOptions dispatchOptions)
        {
            var address = GetAddress(dispatchOptions);

            var messageSender = (IMessageSender)senders.Get(address.Path, address.Namespace.ConnectionString);

            var brokeredMessages = outgoingMessageConverter.Convert(messages, dispatchOptions);
            await messageSender.SendBatchAsync(brokeredMessages);
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

        void GuardMessageSize(BrokeredMessage brokeredMessage)
        {
            if (brokeredMessage.Size > 256 * 1024)
            {
                throw new MessageTooLargeException($"The message with id {brokeredMessage.MessageId} is larger that the maximum message size allowed by Azure ServiceBus, consider using the databus instead");
            }
        }
    }
}