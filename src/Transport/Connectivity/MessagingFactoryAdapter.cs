namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    class MessagingFactoryAdapter : IMessagingFactory
    {
        readonly MessagingFactory _factory;

        public MessagingFactoryAdapter(MessagingFactory factory)
        {
            _factory = factory;
        }

        public bool IsClosed
        {
            get { return _factory.IsClosed; }
        }

        public RetryPolicy RetryPolicy
        {
            get { return _factory.RetryPolicy; }
            set { _factory.RetryPolicy = value; }
        }

        public async Task<IMessageReceiver> CreateMessageReceiverAsync(string entitypath, ReceiveMode receiveMode)
        {
            return new MessageReceiverAdapter(await _factory.CreateMessageReceiverAsync(entitypath, receiveMode));
        }

        public async Task<IMessageSender> CreateMessageSenderAsync(string entitypath)
        {
            return new MessageSenderAdapter(await _factory.CreateMessageSenderAsync(entitypath));
        }

        public async Task<IMessageSender> CreateMessageSenderAsync(string entitypath, string viaEntityPath)
        {
            return new MessageSenderAdapter(await _factory.CreateMessageSenderAsync(entitypath, viaEntityPath));
        }
    }
}