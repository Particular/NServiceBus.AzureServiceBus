namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Settings;

    class MessageSenderCreator : ICreateMessageSendersInternal
    {
        public MessageSenderCreator(IManageMessagingFactoryLifeCycleInternal factories, ReadOnlySettings settings)
        {
            this.factories = factories;
            this.settings = settings;
        }


        public async Task<IMessageSenderInternal> Create(string entitypath, string viaEntityPath, string namespaceName)
        {
            var factory = factories.Get(namespaceName);

            var sender = viaEntityPath != null
                ? await factory.CreateMessageSender(entitypath, viaEntityPath).ConfigureAwait(false)
                : await factory.CreateMessageSender(entitypath).ConfigureAwait(false);

            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryPolicy))
            {
                sender.RetryPolicy = settings.Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryPolicy);
            }
            return sender;
        }

        IManageMessagingFactoryLifeCycleInternal factories;
        ReadOnlySettings settings;
    }
}