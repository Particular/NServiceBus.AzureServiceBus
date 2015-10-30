namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    
    public class MessageSenderAdapter : IMessageSender
    {
        readonly MessageSender _sender;

        public MessageSenderAdapter(MessageSender sender)
        {
            _sender = sender;
        }

        public bool IsClosed
        {
            get { return _sender.IsClosed; }
        }

        public RetryPolicy RetryPolicy
        {
            get { return _sender.RetryPolicy; }
            set { _sender.RetryPolicy = value; }
        }

        public Task SendAsync(BrokeredMessage message)
        {
          return _sender.SendAsync(message);
        }

        public Task SendBatchAsync(IEnumerable<BrokeredMessage> messages)
        {
            return _sender.SendBatchAsync(messages);
        }

        public Task CloseAsync()
        {
            return _sender.CloseAsync();
        }
    }
}