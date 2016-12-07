namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    class MessagingFactoryAdapter : IMessagingFactoryInternal
    {
        MessagingFactory factory;

        public MessagingFactoryAdapter(MessagingFactory factory)
        {
            this.factory = factory;
        }

        public bool IsClosed => factory.IsClosed;

        public RetryPolicy RetryPolicy
        {
            get { return factory.RetryPolicy; }
            set { factory.RetryPolicy = value; }
        }

        public async Task<IMessageReceiverInternal> CreateMessageReceiver(string entitypath, ReceiveMode receiveMode)
        {
            return new MessageReceiverAdapter(await factory.CreateMessageReceiverAsync(entitypath, receiveMode).ConfigureAwait(false));
        }

        public async Task<IMessageSenderInternal> CreateMessageSender(string entitypath)
        {
            return new MessageSenderAdapter(await factory.CreateMessageSenderAsync(entitypath).ConfigureAwait(false));
        }

        public async Task<IMessageSenderInternal> CreateMessageSender(string entitypath, string viaEntityPath)
        {
            return new MessageSenderAdapter(await factory.CreateMessageSenderAsync(entitypath, viaEntityPath).ConfigureAwait(false));
        }

        public Task CloseAsync()
        {
            return factory.CloseAsync();
        }
    }
}