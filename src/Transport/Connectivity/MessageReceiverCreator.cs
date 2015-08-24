namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Settings;

    class MessageReceiverCreator : ICreateClientEntities
    {
        readonly IManageMessagingFactoryLifeCycle _factories;
        readonly ReadOnlySettings _settings;

        public MessageReceiverCreator(IManageMessagingFactoryLifeCycle factories, ReadOnlySettings settings)
        {
            this._factories = factories;
            _settings = settings;
        }


        public async Task<IClientEntity> CreateAsync(string entitypath, string connectionstring)
        {
            var factory = _factories.Get(connectionstring);
            var receiveMode = _settings.Get<ReceiveMode>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode);

            var receiver = await factory.CreateMessageReceiverAsync(entitypath, receiveMode);

            if (_settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount))
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