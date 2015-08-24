namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    public class MessageReceiverAdapter : IMessageReceiver
    {
        readonly MessageReceiver _receiver;

        public MessageReceiverAdapter(MessageReceiver receiver)
        {
            _receiver = receiver;
        }

        public bool IsClosed
        {
            get { return _receiver.IsClosed; }
        }

        public RetryPolicy RetryPolicy
        {
            get { return _receiver.RetryPolicy; }
            set { _receiver.RetryPolicy = value; }
        }

        public int PrefetchCount
        {
            get { return _receiver.PrefetchCount; }
            set { _receiver.PrefetchCount = value; }
        }

        public ReceiveMode Mode
        {
            get { return _receiver.Mode; }
        }
    }
}