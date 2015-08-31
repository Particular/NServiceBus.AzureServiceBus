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

        public async Task SendAsync(BrokeredMessage message)
        {
            await _sender.SendAsync(message);
        }

        public async Task SendBatchAsync(IEnumerable<BrokeredMessage> messages)
        {
            await _sender.SendBatchAsync(messages);
        }

        public Task CloseAsync()
        {
            return _sender.CloseAsync();
        }
    }
}