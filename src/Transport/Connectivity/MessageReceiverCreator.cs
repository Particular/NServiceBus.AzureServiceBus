namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Logging;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Settings;

    class MessageReceiverCreator : ICreateMessageReceivers
    {
        static ILog logger = LogManager.GetLogger(typeof(MessageReceiverCreator));
        IManageMessagingFactoryLifeCycle factories;
        ReadOnlySettings settings;

        public MessageReceiverCreator(IManageMessagingFactoryLifeCycle factories, ReadOnlySettings settings)
        {
            this.factories = factories;
            this.settings = settings;
        }


        public async Task<IMessageReceiver> Create(string entityPath, string namespaceAlias)
        {
            var factory = factories.Get(namespaceAlias);
            var receiveMode = settings.Get<ReceiveMode>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode);

            var prefetchCount = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount);
            var userHasNotProvidedPrefetchCount = !settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount);
            var transportTransactionMode = settings.HasExplicitValue<TransportTransactionMode>() ? settings.Get<TransportTransactionMode>() : settings.SupportedTransactionMode();

            // do not allow prefetch if user hasn't explicitly instructed to use a prefetch count of a certain size when ReceiveAndDelete mode is used (TransTxMode.None)
            if (userHasNotProvidedPrefetchCount && (transportTransactionMode == TransportTransactionMode.None || receiveMode == ReceiveMode.ReceiveAndDelete))
            {
                prefetchCount = 0;
                logger.Warn("Mesasge receiver PrefetchCount was reduced to zero because to avoid message loss with ReceiveAndDelete receive mode. To enforce prefetch, use configuration API to set the value explicitly.");
            }

            var receiver = await factory.CreateMessageReceiver(entityPath, receiveMode).ConfigureAwait(false);
            receiver.PrefetchCount = prefetchCount;
            
            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessageReceivers.RetryPolicy))
            {
                receiver.RetryPolicy = settings.Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.RetryPolicy);
            }
            return receiver;
        }
    }
}