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
            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryPolicy))
            {
                retryPolicy = settings.Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryPolicy);
            }
        }


        public async Task<IMessageSenderInternal> Create(string entitypath, string viaEntityPath, string namespaceName)
        {
            var factory = factories.Get(namespaceName);

            var sender = viaEntityPath != null
                ? await factory.CreateMessageSender(entitypath, viaEntityPath).ConfigureAwait(false)
                : await factory.CreateMessageSender(entitypath).ConfigureAwait(false);

            if (retryPolicy != null)
            {
                sender.RetryPolicy = retryPolicy;
            }
            return sender;
        }

        IManageMessagingFactoryLifeCycleInternal factories;
        RetryPolicy retryPolicy;
    }
}