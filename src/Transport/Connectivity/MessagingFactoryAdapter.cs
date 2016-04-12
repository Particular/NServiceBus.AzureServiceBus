namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    class MessagingFactoryAdapter : IMessagingFactory
    {
        readonly MessagingFactory factory;

        public MessagingFactoryAdapter(MessagingFactory factory)
        {
            this.factory = factory;
        }

        public bool IsClosed
        {
            get { return factory.IsClosed; }
        }

        public RetryPolicy RetryPolicy
        {
            get { return factory.RetryPolicy; }
            set { factory.RetryPolicy = value; }
        }

        public async Task<IMessageReceiver> CreateMessageReceiver(string entitypath, ReceiveMode receiveMode)
        {
            return new MessageReceiverAdapter(await factory.CreateMessageReceiverAsync(entitypath, receiveMode).ConfigureAwait(false));
        }

        public async Task<IMessageSender> CreateMessageSender(string entitypath)
        {
            return new MessageSenderAdapter(await factory.CreateMessageSenderAsync(entitypath).ConfigureAwait(false));
        }

        public async Task<IMessageSender> CreateMessageSender(string entitypath, string viaEntityPath)
        {
            return new MessageSenderAdapter(await factory.CreateMessageSenderAsync(entitypath, viaEntityPath).ConfigureAwait(false));
        }

        public async Task CloseAsync()
        {
            await factory.CloseAsync().ConfigureAwait(false);
        }
    }
}