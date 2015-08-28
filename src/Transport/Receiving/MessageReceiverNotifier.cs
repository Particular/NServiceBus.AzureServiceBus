namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Logging;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    class MessageReceiverNotifier : INotifyIncomingMessages
    {
        readonly IManageClientEntityLifeCycle clientEntities;
        readonly IConvertBrokeredMessagesToIncomingMessages brokeredMessageConverter;
        readonly ReadOnlySettings settings;
        IMessageReceiver internalReceiver;
        OnMessageOptions options;
        Func<IncomingMessage, Task> callback;

        ILog logger = LogManager.GetLogger<MessageReceiverNotifier>();
        

        public MessageReceiverNotifier(IManageClientEntityLifeCycle clientEntities, IConvertBrokeredMessagesToIncomingMessages brokeredMessageConverter, ReadOnlySettings settings)
        {
            this.clientEntities = clientEntities;
            this.brokeredMessageConverter = brokeredMessageConverter;
            this.settings = settings;
        }

        public void Initialize(string entitypath, string connectionstring, Func<IncomingMessage, Task> callback, int maximumConcurrency)
        {
            this.callback = callback;

            internalReceiver = clientEntities.Get(entitypath, connectionstring) as IMessageReceiver;

            if (internalReceiver == null)
            {
                throw new Exception(string.Format("MessageReceiverNotifier did not get a MessageReceiver instance for entity path {0}, this is probably due to a misconfiguration of the topology", entitypath));
            }

            options = new OnMessageOptions
            {
                AutoComplete = true,
                AutoRenewTimeout = settings.Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.AutoRenewTimeout),
                MaxConcurrentCalls = maximumConcurrency
            };

            options.ExceptionReceived += OptionsOnExceptionReceived;
        }

        void OptionsOnExceptionReceived(object sender, ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            // todo respond appropriately

            // according to blog posts, this method is invoked on
            //- Exceptions raised during the time that the message pump is waiting for messages to arrive on the service bus
            //- Exceptions raised during the time that your code is processing the BrokeredMessage
            //- And, it is raised when the receive process successfully completes.

            logger.InfoFormat("OptionsOnExceptionReceived invoked, action: {0}, exception: {0}", exceptionReceivedEventArgs.Action, exceptionReceivedEventArgs.Exception);
        }


        public Task Start()
        {
            internalReceiver.OnMessageAsync(message => callback(brokeredMessageConverter.Convert(message)), options);

            return Task.FromResult(true);
        }

        public async Task Stop()
        {
            await internalReceiver.CloseAsync();
        }
    }
}