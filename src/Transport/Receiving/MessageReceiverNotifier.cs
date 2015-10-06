namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Logging;
    using Settings;
    using Transports;

    class MessageReceiverNotifier : INotifyIncomingMessages
    {
        readonly IManageClientEntityLifeCycle clientEntities;
        readonly IConvertBrokeredMessagesToIncomingMessages brokeredMessageConverter;
        readonly ReadOnlySettings settings;
        IMessageReceiver internalReceiver;
        OnMessageOptions options;
        Func<IncomingMessage, ReceiveContext, Task> incoming;
        Func<Exception, Task> error;
        string path;
        string connstring;
        bool stopping = false;

        ILog logger = LogManager.GetLogger<MessageReceiverNotifier>();

        public MessageReceiverNotifier(IManageClientEntityLifeCycle clientEntities, IConvertBrokeredMessagesToIncomingMessages brokeredMessageConverter, ReadOnlySettings settings)
        {
            this.clientEntities = clientEntities;
            this.brokeredMessageConverter = brokeredMessageConverter;
            this.settings = settings;
        }

        public bool IsRunning { get; private set; }
        public int RefCount { get; set; }

        public void Initialize(string entitypath, string connectionstring, Func<IncomingMessage, ReceiveContext, Task> callback, Func<Exception, Task> errorCallback, int maximumConcurrency)
        {
            this.incoming = callback;
            this.error = errorCallback;
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

        async void OptionsOnExceptionReceived(object sender, ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            // todo respond appropriately

            // according to blog posts, this method is invoked on
            //- Exceptions raised during the time that the message pump is waiting for messages to arrive on the service bus
            //- Exceptions raised during the time that your code is processing the BrokeredMessage
            //- It is raised when the receive process successfully completes. (Does not seem to be the case)

            if (!stopping) //- It is raised when the underlying connection closes because of our close operation 
            {
                logger.InfoFormat("OptionsOnExceptionReceived invoked, action: {0}, exception: {1}", exceptionReceivedEventArgs.Action, exceptionReceivedEventArgs.Exception);

                if (error != null)
                {
                    await error(exceptionReceivedEventArgs.Exception);
                }
            }
        }
        
        public Task Start()
        {
            stopping = false;

            internalReceiver = clientEntities.Get(path, connstring) as IMessageReceiver;

            if (internalReceiver == null)
            {
                throw new Exception(string.Format("MessageReceiverNotifier did not get a MessageReceiver instance for entity path {0}, this is probably due to a misconfiguration of the topology", path));
            }

            internalReceiver.OnMessageAsync(async message => await ProcessMessage(message), options);

            IsRunning = true;

            return Task.FromResult(true);
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
            await incoming(incomingMessage, context) //invoke pipeline

            //processing success, invoke completion callbacks
            .ContinueWith(async task => await InvokeCompletionCallbacks(message, context), TaskContinuationOptions.OnlyOnRanToCompletion)

            //processing failure: error handler is called for us, just abandon brokeredmessage
            .ContinueWith(async t => await Abandon(message, t), TaskContinuationOptions.OnlyOnFaulted);

        }

        async Task InvokeCompletionCallbacks(BrokeredMessage message, BrokeredMessageReceiveContext context)
        {
            //// call completion callbacks
            var tasksToComplete = context.OnComplete.Select(async toComplete => await toComplete()).ToList();

            var completeTask = Task.WhenAll(tasksToComplete);
            
#pragma warning disable 4014
            //completion success: complete brokeredmessage
            completeTask.ContinueWith(async t => await Complete(message), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.AttachedToParent);
            //completion failure: call error handler & abandon brokeredmessage
            completeTask.ContinueWith(async t => await Abandon(message, t), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.AttachedToParent);
#pragma warning restore 4014

            await completeTask;
        }

        async Task Abandon(BrokeredMessage message, Task t)
        {
            logger.InfoFormat("Exceptions occured OnComplete, exception: {0}", t.Exception);

            if (error != null)
            {
                await error(t.Exception);
            }

            await message.AbandonAsync();
            //message.Abandon();
        }

        async Task Complete(BrokeredMessage message)
        {
            logger.InfoFormat("Completing brokered message");

            await message.CompleteAsync();
            //message.Complete();

            //return TaskEx.Completed;
        }

        public async Task Stop()
        {
            stopping = true;

            await internalReceiver.CloseAsync();

            IsRunning = false;
        }
    }

}