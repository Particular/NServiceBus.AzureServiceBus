namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;
    using Microsoft.ServiceBus.Messaging;
    using Logging;
    using Settings;
    
    class MessageReceiverNotifier : INotifyIncomingMessages
    {
        readonly IManageMessageReceiverLifeCycle clientEntities;
        readonly IConvertBrokeredMessagesToIncomingMessages brokeredMessageConverter;
        readonly ReadOnlySettings settings;
        IMessageReceiver internalReceiver;
        OnMessageOptions options;
        Func<IncomingMessageDetails, ReceiveContext, Task> incomingCallback;
        Func<Exception, Task> errorCallback;
        string path;
        string connstring;
        bool stopping = false;

        static ILog logger = LogManager.GetLogger<MessageReceiverNotifier>();

        public MessageReceiverNotifier(IManageMessageReceiverLifeCycle clientEntities, IConvertBrokeredMessagesToIncomingMessages brokeredMessageConverter, ReadOnlySettings settings)
        {
            this.clientEntities = clientEntities;
            this.brokeredMessageConverter = brokeredMessageConverter;
            this.settings = settings;
        }

        public bool IsRunning { get; private set; }
        public int RefCount { get; set; }

        public void Initialize(string entitypath, string connectionstring, Func<IncomingMessageDetails, ReceiveContext, Task> callback, Func<Exception, Task> errorCallback, int maximumConcurrency)
        {
            this.incomingCallback = callback;
            this.errorCallback = errorCallback;
            this.path = entitypath;
            this.connstring = connectionstring;

            options = new OnMessageOptions
            {
                AutoComplete = false,
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
            //- It is raised when the receive process successfully completes. (Does not seem to be the case)

            if (!stopping) //- It is raised when the underlying connection closes because of our close operation 
            {
                logger.InfoFormat("OptionsOnExceptionReceived invoked, action: {0}, exception: {1}", exceptionReceivedEventArgs.Action, exceptionReceivedEventArgs.Exception);

                errorCallback?.Invoke(exceptionReceivedEventArgs.Exception).GetAwaiter().GetResult();
            }
        }
        
        public void Start()
        {
            stopping = false;

            internalReceiver = clientEntities.Get(path, connstring);

            if (internalReceiver == null)
            {
                throw new Exception($"MessageReceiverNotifier did not get a MessageReceiver instance for entity path {path}, this is probably due to a misconfiguration of the topology");
            }

            internalReceiver.OnMessage(message => ProcessMessage(message), options);

            IsRunning = true;
        }

        async Task ProcessMessage(BrokeredMessage message)
        {
            var incomingMessage = brokeredMessageConverter.Convert(message);
            var context = new BrokeredMessageReceiveContext()
            {
                BrokeredMessage = message,
                EntityPath = path,
                ConnectionString = connstring,
                ReceiveMode = internalReceiver.Mode,
                OnComplete = new List<Func<Task>>()
            };

            try
            {
                await incomingCallback(incomingMessage, context).ConfigureAwait(false);
                await InvokeCompletionCallbacksAsync(message, context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await AbandonAsync(message, exception).ConfigureAwait(false);
            }
        }

        async Task InvokeCompletionCallbacksAsync(BrokeredMessage message, BrokeredMessageReceiveContext context)
        {
            // send via receive queue only works when wrapped in a scope
            var useTx = settings.Get<bool>(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue);
            using (var scope = useTx ? new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled) : null)
            {
                await Task.WhenAll(context.OnComplete.Select(toComplete => toComplete()).ToList()).ConfigureAwait(false);
                await Complete(message).ConfigureAwait(false);
                logger.InfoFormat("Completed, completing scope if present");
                scope?.Complete();
            }
        }

        async Task AbandonAsync(BrokeredMessage message, Exception exception)
        {
            logger.Info($"Exceptions occurred OnComplete, exception: {exception}");

            using (var suppressScope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            {
                logger.InfoFormat("Abandoning brokered message");

                await message.AbandonAsync().ConfigureAwait(false);

                suppressScope.Complete();
            }

            if (errorCallback != null)
            {
                await errorCallback(exception).ConfigureAwait(false);
            }
        }

        Task Complete(BrokeredMessage message)
        {
            logger.InfoFormat("Completing brokered message");

            return message.CompleteAsync();
        }

        public async Task StopAsync()
        {
            stopping = true;

            await internalReceiver.CloseAsync().ConfigureAwait(false);

            IsRunning = false;
        }
    }

}