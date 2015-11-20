namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using NServiceBus.Settings;

    class MessageSenderCreator : ICreateMessageSenders
    {
        readonly IManageMessagingFactoryLifeCycle _factories;
        readonly ReadOnlySettings _settings;

        public MessageSenderCreator(IManageMessagingFactoryLifeCycle factories, ReadOnlySettings settings)
        {
            this._factories = factories;
            _settings = settings;
        }


        public async Task<IMessageSender> Create(string entitypath, string viaEntityPath, string connectionstring)
        {
            var factory = _factories.Get(connectionstring);

            var sender = viaEntityPath != null 
                ? await factory.CreateMessageSender(entitypath, viaEntityPath).ConfigureAwait(false)
                : await factory.CreateMessageSender(entitypath).ConfigureAwait(false);

            if (_settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryPolicy))
            {
                sender.RetryPolicy = _settings.Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryPolicy);
            }
            return sender;

        }
    }
}