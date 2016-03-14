namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Settings;

    class MessageReceiverCreator : ICreateMessageReceivers
    {
        readonly IManageMessagingFactoryLifeCycle _factories;
        readonly ReadOnlySettings _settings;

        public MessageReceiverCreator(IManageMessagingFactoryLifeCycle factories, ReadOnlySettings settings)
        {
            _factories = factories;
            _settings = settings;
        }


        public async Task<IMessageReceiver> Create(string entitypath, string namespaceName)
        {
            var factory = _factories.Get(namespaceName);
            var receiveMode = _settings.Get<ReceiveMode>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode);

            var receiver = await factory.CreateMessageReceiver(entitypath, receiveMode).ConfigureAwait(false);

            if (_settings.HasSetting(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount))
            {
                receiver.PrefetchCount = _settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount);
            }
            if (_settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessageReceivers.RetryPolicy))
            {
                receiver.RetryPolicy = _settings.Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.RetryPolicy);
            }
            return receiver;

        }
    }
}