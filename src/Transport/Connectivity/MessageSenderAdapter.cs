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
                // as Transaction.Current is static, there is a high risk of transferring the
                // state into other threads that use Transaction.Current
                Transaction.Current = transaction;
                // the AsyncResult created inside the ASB sdk copies over the references from Transaction.Current
                var task = sender.SendBatchAsync(messages);
                // now we can get rid of it again
                Transaction.Current = null;
                // and await the result
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