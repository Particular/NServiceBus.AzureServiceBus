namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus;
    using NServiceBus.Transports;

    public class DefaultOutgoingMessageSender : ISendOutgoingMessages
    {
        readonly IConvertOutgoingMessagesToBrokeredMessages outgoingMessageConverter;
        readonly IMessageSender messageSender;

        public DefaultOutgoingMessageSender(IConvertOutgoingMessagesToBrokeredMessages outgoingMessageConverter, IMessageSender messageSender)
        {
            this.outgoingMessageConverter = outgoingMessageConverter;
            this.messageSender = messageSender;
        }

        public async Task SendAsync(OutgoingMessage message, DispatchOptions dispatchOptions)
        {
            var brokeredMessage = outgoingMessageConverter.Convert(message, dispatchOptions);
            await messageSender.SendAsync(brokeredMessage);
        }

        public async Task SendBatchAsync(IEnumerable<OutgoingMessage> messages, DispatchOptions dispatchOptions)
        {
            var brokeredMessages = outgoingMessageConverter.Convert(messages, dispatchOptions);
            await messageSender.SendBatchAsync(brokeredMessages);
        }

        void GuardMessageSize(BrokeredMessage brokeredMessage)
        {
            if (brokeredMessage.Size > 256 * 1024)
            {
                throw new MessageTooLargeException(string.Format("The message with id {0} is larger that the maximum message size allowed by Azure ServiceBus, consider using the databus instead", brokeredMessage.MessageId));
            }
        }
    }
}