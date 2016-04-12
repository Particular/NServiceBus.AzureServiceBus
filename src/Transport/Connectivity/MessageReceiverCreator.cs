namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Settings;

    class MessageReceiverCreator : ICreateMessageReceivers
    {
        IManageMessagingFactoryLifeCycle factories;
        ReadOnlySettings settings;

        public MessageReceiverCreator(IManageMessagingFactoryLifeCycle factories, ReadOnlySettings settings)
        {
            this.factories = factories;
            this.settings = settings;
        }


        public async Task<IMessageReceiver> Create(string entitypath, string namespaceName)
        {
            var factory = factories.Get(namespaceName);
            var receiveMode = settings.Get<ReceiveMode>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode);

            var receiver = await factory.CreateMessageReceiver(entitypath, receiveMode).ConfigureAwait(false);

            if (settings.HasSetting(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount))
            {
                receiver.PrefetchCount = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount);
            }
            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessageReceivers.RetryPolicy))
            {
                receiver.RetryPolicy = settings.Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.RetryPolicy);
            }
            return receiver;
        }
    }
}