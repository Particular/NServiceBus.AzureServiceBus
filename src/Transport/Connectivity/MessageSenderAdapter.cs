namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Transactions;
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

        public async Task SendBatch(IEnumerable<BrokeredMessage> messages, CommittableTransaction transaction)
        {
            try
            {
                Transaction.Current = transaction;
                var task = sender.SendBatchAsync(messages);
                Transaction.Current = null;
                await task.ConfigureAwait(false);
            }
            finally
            {
                Transaction.Current = null;
            }
        }

        public Task CloseAsync()
        {
            return sender.CloseAsync();
        }
    }
}