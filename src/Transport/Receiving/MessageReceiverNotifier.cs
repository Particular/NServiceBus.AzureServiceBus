namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using Microsoft.ServiceBus.Messaging;
    using Logging;
    using Azure.Transports.WindowsAzureServiceBus;
    using Settings;

    class MessageReceiverNotifier : INotifyIncomingMessages
    {
        IManageMessageReceiverLifeCycle clientEntities;
        IConvertBrokeredMessagesToIncomingMessages brokeredMessageConverter;
        ReadOnlySettings settings;
        IMessageReceiver internalReceiver;
        ReceiveMode receiveMode;
        OnMessageOptions options;
        Func<IncomingMessageDetails, ReceiveContext, Task> incomingCallback;
        Func<Exception, Task> errorCallback;
        ConcurrentDictionary<Task, Task> pipelineInvocationTasks;
        string fullPath;
        EntityInfo entity;
        bool stopping;
        ConcurrentQueue<Guid> locksIdsToComplete;
        CancellationTokenSource lockIdCompletionCts;

        static ILog logger = LogManager.GetLogger<MessageReceiverNotifier>();

        public MessageReceiverNotifier(IManageMessageReceiverLifeCycle clientEntities, IConvertBrokeredMessagesToIncomingMessages brokeredMessageConverter, ReadOnlySettings settings)
        {
            this.clientEntities = clientEntities;
            this.brokeredMessageConverter = brokeredMessageConverter;
            this.settings = settings;
            locksIdsToComplete = new ConcurrentQueue<Guid>();
            lockIdCompletionCts = new CancellationTokenSource();
        }

        public bool IsRunning { get; private set; }
        public int RefCount { get; set; }

        public void Initialize(EntityInfo entity, Func<IncomingMessageDetails, ReceiveContext, Task> callback, Func<Exception, Task> errorCallback, int maximumConcurrency)
        {
            receiveMode = settings.Get<ReceiveMode>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode);

            incomingCallback = callback;
            this.errorCallback = errorCallback;
            this.entity = entity;

            fullPath = entity.Path;
            if (entity.Type == EntityType.Subscription)
            {
                var topic = entity.RelationShips.First(r => r.Type == EntityRelationShipType.Subscription);
                fullPath = SubscriptionClient.FormatSubscriptionPath(topic.Target.Path, entity.Path);
            }

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
            pipelineInvocationTasks = new ConcurrentDictionary<Task, Task>();

            internalReceiver = clientEntities.Get(fullPath, entity.Namespace.Name);

            if (internalReceiver == null)
            {
                throw new Exception($"MessageReceiverNotifier did not get a MessageReceiver instance for entity path {fullPath}, this is probably due to a misconfiguration of the topology");
            }

            Func<BrokeredMessage, Task> callback = message =>
            {
                var processTask = ProcessMessageAsync(message);
                pipelineInvocationTasks.TryAdd(processTask, processTask);
                processTask.ContinueWith(t =>
                {
                    Task toBeRemoved;
                    pipelineInvocationTasks.TryRemove(t, out toBeRemoved);
                }, TaskContinuationOptions.ExecuteSynchronously);
                return processTask;
            };

            IsRunning = true;

            internalReceiver.OnMessage(callback, options);

            StartCompletionTask();
        }

        void StartCompletionTask()
        {
            Task.Factory.StartNew(async () =>
            {
                while (!lockIdCompletionCts.Token.IsCancellationRequested)
                {
                    if (locksIdsToComplete.IsEmpty)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
                        continue;
                    }

                    var ids = new List<Guid>();
                    for (var i = 0; i < locksIdsToComplete.Count; i++)
                    {
                        Guid dummy;
                        locksIdsToComplete.TryDequeue(out dummy);
                        ids.Add(dummy);
                    }

                    await internalReceiver.CompleteBatchAsync(ids).ConfigureAwait(false);
                }
            }, lockIdCompletionCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        async Task ProcessMessageAsync(BrokeredMessage message)
        {
            if (stopping || !IsRunning)
            {
                await AbandonAsync(message).ConfigureAwait(false);
                return;
            }

            IncomingMessageDetails incomingMessage;
            try
            {
                incomingMessage = brokeredMessageConverter.Convert(message);
            }
            catch (UnsupportedBrokeredMessageBodyTypeException exception)
            {
                await message.DeadLetterAsync("BrokeredMessage to IncomingMessageDetails conversion failure", exception.ToString()).ConfigureAwait(false);
                return;
            }

            var context = new BrokeredMessageReceiveContext
            {
                IncomingBrokeredMessage = message,
                Entity = entity,
                ReceiveMode = internalReceiver.Mode,
                OnComplete = new List<Func<Task>>()
            };

            try
            {
                await incomingCallback(incomingMessage, context).ConfigureAwait(false);

                if (context.CancellationToken.IsCancellationRequested)
                {
                    await AbandonAsyncOnCancellation(message).ConfigureAwait(false);
                }
                else
                {
                    await InvokeCompletionCallbacksAsync(context).ConfigureAwait(false);
                }

                //await message.SafeCompleteAsync().ConfigureAwait(false);
                locksIdsToComplete.Enqueue(message.LockToken);
            }
            catch (Exception exception)
            {
                await AbandonAsync(message, exception).ConfigureAwait(false);
            }
        }

        async Task InvokeCompletionCallbacksAsync(BrokeredMessageReceiveContext context)
        {
            // send via receive queue only works when wrapped in a scope
            var useTx = settings.Get<bool>(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue);
            using (var scope = useTx ? new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled) : null)
            {
                await Task.WhenAll(context.OnComplete.Select(toComplete => toComplete()).ToList()).ConfigureAwait(false);
                scope?.Complete();
            }
        }

        Task AbandonAsync(BrokeredMessage message)
        {
            logger.Info("Received message while shutting down, abandoning it so we can process it later.");

            return AbandonInternal(message);
        }

        Task AbandonAsyncOnCancellation(BrokeredMessage message)
        {
            logger.Info("Received message is cancelled by the pipeline, abandoning it so we can process it later.");

            return AbandonInternal(message);
        }

        async Task AbandonAsync(BrokeredMessage message, Exception exception)
        {
            logger.Info($"Exceptions occurred OnComplete, exception: {exception}");

            await AbandonInternal(message).ConfigureAwait(false);

            if (errorCallback != null && exception != null)
            {
                await errorCallback(exception).ConfigureAwait(false);
            }
        }

        async Task AbandonInternal(BrokeredMessage message)
        {
            if (receiveMode == ReceiveMode.ReceiveAndDelete) return;

            using (var suppressScope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            {
                logger.InfoFormat("Abandoning brokered message {0}", message.MessageId);

                await message.SafeAbandonAsync().ConfigureAwait(false);

                suppressScope.Complete();
            }
        }

        public async Task Stop()
        {
            stopping = true;

            logger.Info("Stopping notifier for " + fullPath);

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
            var allTasks = pipelineInvocationTasks.Values;
            var finishedTask = await Task.WhenAny(Task.WhenAll(allTasks), timeoutTask).ConfigureAwait(false);

            if (finishedTask.Equals(timeoutTask))
            {
                logger.Error("The receiver failed to stop with in the time allowed (30s)");
            }

            // drain the last messages before signaling to stop batched completion task
            while (!locksIdsToComplete.IsEmpty) {}
            lockIdCompletionCts.Cancel();

            pipelineInvocationTasks.Clear();

            await internalReceiver.CloseAsync().ConfigureAwait(false);

            logger.Info("Notifier for " + fullPath + " stopped");

            IsRunning = false;
        }
    }

}