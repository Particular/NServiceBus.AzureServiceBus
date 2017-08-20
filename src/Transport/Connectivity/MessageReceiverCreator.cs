namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Settings;

    class MessageReceiverCreator : ICreateMessageReceiversInternal
    {
        public MessageReceiverCreator(IManageMessagingFactoryLifeCycleInternal factories, ReadOnlySettings settings)
        {
            this.factories = factories;
            receiveMode = settings.Get<ReceiveMode>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode);
            prefetchCount = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount);
            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessageReceivers.RetryPolicy))
            {
                retryPolicy = settings.Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.RetryPolicy);
            }
        }

        public async Task<IMessageReceiverInternal> Create(string entityPath, string namespaceAlias)
        {
            var factory = factories.Get(namespaceAlias);
            var receiver = await factory.CreateMessageReceiver(entityPath, receiveMode).ConfigureAwait(false);
            receiver.PrefetchCount = prefetchCount;

            if (retryPolicy != null)
            {
                receiver.RetryPolicy = retryPolicy;
            }
            return receiver;
        }

        IManageMessagingFactoryLifeCycleInternal factories;
        ReceiveMode receiveMode;
        int prefetchCount;
        RetryPolicy retryPolicy;
    }
}