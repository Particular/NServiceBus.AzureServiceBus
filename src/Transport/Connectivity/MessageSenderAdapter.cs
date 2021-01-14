namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    class MessageSenderAdapter : IMessageSenderInternal
    {
        public MessageSenderAdapter(MessageSender sender)
        {
            this.sender = sender;
        }

        public bool IsClosed => sender.IsClosed;

        public RetryPolicy RetryPolicy
        {
            get => sender.RetryPolicy;
            set => sender.RetryPolicy = value;
        }

        public Task Send(BrokeredMessage message) => sender.SendAsync(message);

        public Task SendBatch(IEnumerable<BrokeredMessage> messages) => sender.SendBatchAsync(messages);

        public Task CloseAsync() => sender.CloseAsync();

        MessageSender sender;
    }
}