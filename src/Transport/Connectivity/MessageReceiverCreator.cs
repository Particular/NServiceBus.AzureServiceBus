namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Settings;

    class MessageReceiverCreator : ICreateMessageReceivers
    {
        ICreateMessagingFactories factories;
        ReadOnlySettings settings;

        public MessageReceiverCreator(ICreateMessagingFactories factories, ReadOnlySettings settings)
        {
            this.factories = factories;
            this.settings = settings;
        }

        public async Task<IMessageReceiver> Create(string entityPath, string namespaceAlias)
        {
            var factory = factories.Create(namespaceAlias);
            var receiveMode = settings.Get<ReceiveMode>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode);

            var receiver = await factory.CreateMessageReceiver(entityPath, receiveMode).ConfigureAwait(false);

            receiver.PrefetchCount = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount);

            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessageReceivers.RetryPolicy))
            {
                receiver.RetryPolicy = settings.Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.RetryPolicy);
            }
            return receiver;
        }
    }
}