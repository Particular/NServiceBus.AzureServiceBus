namespace NServiceBus.AzureServiceBus
{
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
    }
}