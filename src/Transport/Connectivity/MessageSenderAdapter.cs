namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    class MessageSenderAdapter : IMessageSender
    {
        MessageSender sender;

        public MessageSenderAdapter(MessageSender sender)
        {
            this.sender = sender;
        }

        public bool IsClosed => sender.IsClosed;

        public RetryPolicy RetryPolicy
        {
            get { return sender.RetryPolicy; }
            set { sender.RetryPolicy = value; }
        }

        public Task Send(BrokeredMessage message)
        {
          return sender.SendAsync(message);
        }

        public Task SendBatch(IEnumerable<BrokeredMessage> messages)
        {
            return sender.SendBatchAsync(messages);
        }

        public Task CloseAsync()
        {
            return sender.CloseAsync();
        }
    }
}