namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using NServiceBus.Settings;

    class MessageSenderCreator : ICreateClientEntities
    {
        readonly IManageMessagingFactoryLifeCycle _factories;
        readonly ReadOnlySettings _settings;

        public MessageSenderCreator(IManageMessagingFactoryLifeCycle factories, ReadOnlySettings settings)
        {
            this._factories = factories;
            _settings = settings;
        }


        public async Task<IClientEntity> CreateAsync(string entitypath, string connectionstring)
        {
            var factory = _factories.Get(connectionstring);

            var sender = await factory.CreateMessageSenderAsync(entitypath);

            if (_settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryPolicy))
            {
                sender.RetryPolicy = _settings.Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryPolicy);
            }
            return sender;

        }
    }
}