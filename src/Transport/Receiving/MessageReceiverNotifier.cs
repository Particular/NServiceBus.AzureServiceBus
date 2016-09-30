namespace NServiceBus.Transport.AzureServiceBus
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
    using Settings;
    using Transport;

    class MessageReceiverNotifier : INotifyIncomingMessages
    {
        IManageMessageReceiverLifeCycle clientEntities;
        IConvertBrokeredMessagesToIncomingMessages brokeredMessageConverter;
        ReadOnlySettings settings;
        IList<IMessageReceiver> internalReceivers = new List<IMessageReceiver>();
        ReceiveMode receiveMode;
        OnMessageOptions options;
        Func<IncomingMessageDetails, ReceiveContext, Task> incomingCallback;
        Func<Exception, Task> errorCallback;
        ConcurrentDictionary<Task, Task> pipelineInvocationTasks;
        string fullPath;
        EntityInfo entity;
        volatile bool stopping;
        volatile bool isRunning;
        ConcurrentStack<Guid> locksTokensToComplete;
        CancellationTokenSource batchedCompletionCts;
        Task batchedCompletionTask;
        static ILog logger = LogManager.GetLogger<MessageReceiverNotifier>();
        Func<ErrorContext, Task<ErrorHandleResult>> processingFailureCallback;

        public MessageReceiverNotifier(IManageMessageReceiverLifeCycle clientEntities, IConvertBrokeredMessagesToIncomingMessages brokeredMessageConverter, ReadOnlySettings settings)
        {
            this.clientEntities = clientEntities;
            this.brokeredMessageConverter = brokeredMessageConverter;
            this.settings = settings;
            locksTokensToComplete = new ConcurrentStack<Guid>();
            batchedCompletionCts = new CancellationTokenSource();
        }

        public bool IsRunning => isRunning;

        public int RefCount { get; set; }

        bool ShouldReceiveMessages => !stopping || isRunning;

        public void Initialize(EntityInfo entity, Func<IncomingMessageDetails, ReceiveContext, Task> callback, Func<Exception, Task> errorCallback, Func<ErrorContext, Task<ErrorHandleResult>> processingFailureCallback, int maximumConcurrency)
        {
            receiveMode = settings.Get<ReceiveMode>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode);

            incomingCallback = callback;
            this.errorCallback = errorCallback;
            this.processingFailureCallback = processingFailureCallback;
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
            // according to blog posts, this method is invoked on
            //- Exceptions raised during the time that the message pump is waiting for messages to arrive on the service bus
            //- Exceptions raised during the time that your code is processing the BrokeredMessage
            //- It is raised when the receive process successfully completes. (Does not seem to be the case)

            if (!stopping) //- It is raised when the underlying connection closes because of our close operation
            {
                var messagingException = exceptionReceivedEventArgs.Exception as MessagingException;
                if (messagingException != null && messagingException.IsTransient)
                {
                    logger.DebugFormat("OptionsOnExceptionReceived invoked, action: '{0}', transient exception with message: {1}", exceptionReceivedEventArgs.Action, messagingException.Detail.Message);

                    // TODO ideally we'd failover to another space if in a certain period of time there are too many transient errors
                }
                else
                {
                    logger.Info($"OptionsOnExceptionReceived invoked, action: '{exceptionReceivedEventArgs.Action}', with non-transient exception.", exceptionReceivedEventArgs.Exception);

                    errorCallback?.Invoke(exceptionReceivedEventArgs.Exception).GetAwaiter().GetResult();
                }
            }
        }

        public void Start()
        {
            stopping = false;
            pipelineInvocationTasks = new ConcurrentDictionary<Task, Task>();

            var numberOfClients = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity);
            for(var i = 0; i < numberOfClients; i++)
            {

                var internalReceiver = clientEntities.Get(fullPath, entity.Namespace.Alias);

                if (internalReceiver == null)
                {
                    throw new Exception($"MessageReceiverNotifier did not get a MessageReceiver instance for entity path {fullPath}, this is probably due to a misconfiguration of the topology");
                }

                Func<BrokeredMessage, Task> callback = message =>
                {
                    var processTask = ProcessMessageAsync(internalReceiver, message);
                    pipelineInvocationTasks.TryAdd(processTask, processTask);
                    processTask.ContinueWith(t =>
                    {
                        Task toBeRemoved;
                        pipelineInvocationTasks.TryRemove(t, out toBeRemoved);
                    }, TaskContinuationOptions.ExecuteSynchronously);
                    return processTask;
                };

                isRunning = true;

                internalReceiver.OnMessage(callback, options);
                PerformBatchedCompletionTask(internalReceiver);

                internalReceivers.Add(internalReceiver);
            }

            
        }

        void PerformBatchedCompletionTask(IMessageReceiver internalReceiver)
        {
            batchedCompletionTask = Task.Run(async () =>
            {
                int count;
                var buffer = new Guid[5000];

                while (!batchedCompletionCts.Token.IsCancellationRequested)
                {
                    if (locksTokensToComplete.IsEmpty)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
                        continue;
                    }

                    count = locksTokensToComplete.TryPopRange(buffer);
                    var tocomplete = buffer.Take(count).ToList();
                    await internalReceiver.SafeCompleteBatchAsync(tocomplete).ConfigureAwait(false);
                    Array.Clear(buffer, 0, buffer.Length);
                }

                // run last check when task has been cancelled to drain remaining lock tokens
                if (locksTokensToComplete.IsEmpty)
                {
                    return;
                }

                count = locksTokensToComplete.TryPopRange(buffer);
                var remainingtocomplete = buffer.Take(count).ToList();
                await internalReceiver.SafeCompleteBatchAsync(remainingtocomplete).ConfigureAwait(false);
                Array.Clear(buffer, 0, buffer.Length);

            }, CancellationToken.None);
        }

        async Task ProcessMessageAsync(IMessageReceiver internalReceiver, BrokeredMessage message)
        {
            if (!ShouldReceiveMessages)
            {
                logger.Info($"Received message with ID {message.MessageId} while shutting down. Message will not be processed and will be retried after {message.LockedUntilUtc}.");
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

            var context = new BrokeredMessageReceiveContext(message, entity, internalReceiver.Mode);

            var transportTransactionMode = settings.HasExplicitValue<TransportTransactionMode>() ? settings.Get<TransportTransactionMode>() : settings.SupportedTransactionMode();
            var wrapInScope = transportTransactionMode == TransportTransactionMode.SendsAtomicWithReceive;
            var completionCanBeBatched = !wrapInScope;
            try
            {
                var scope = wrapInScope ? new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled) : null;
                {
                    using (scope)
                    {
                        await incomingCallback(incomingMessage, context).ConfigureAwait(false);

                        await HandleCompletion(message, context, completionCanBeBatched).ConfigureAwait(false);
                        scope?.Complete();
                    }
                }
            }
            catch (Exception exception) when(ShouldReceiveMessages)
            {
                // and go into recovery mode so that no new messages are added to the transfer queue
                context.Recovering = true;

                // pass the context into the error pipeline
                var transportTransaction = new TransportTransaction();
                transportTransaction.Set(context);
                var errorContext = new ErrorContext(exception, incomingMessage.Headers, incomingMessage.MessageId, incomingMessage.Body, transportTransaction, message.DeliveryCount);

                try
                {
                    var result = await processingFailureCallback(errorContext).ConfigureAwait(false);
                    if (result == ErrorHandleResult.RetryRequired)
                    {
                        await AbandonAsync(message, exception).ConfigureAwait(false);
                    }
                    else
                    {
                        await HandleCompletion(message, context, completionCanBeBatched).ConfigureAwait(false);
                    }
                }
                catch (Exception onErrorException)
                {
                    await AbandonAsync(message, onErrorException).ConfigureAwait(false);
                }
            }
        }

        async Task HandleCompletion(BrokeredMessage message, BrokeredMessageReceiveContext context, bool canbeBatched)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                await AbandonAsyncOnCancellation(message).ConfigureAwait(false);
            }
            else
            {
                if (receiveMode == ReceiveMode.PeekLock)
                {
                    if (canbeBatched)
                    {
                        locksTokensToComplete.Push(message.LockToken);
                    }
                    else
                    {
                        await context.IncomingBrokeredMessage.CompleteAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        Task AbandonAsyncOnCancellation(BrokeredMessage message)
        {
            logger.Info("Received message is cancelled by the pipeline, abandoning it so we can process it later.");

            return AbandonInternal(message);
        }

        async Task AbandonAsync(BrokeredMessage message, Exception exception)
        {
            logger.Info("Exceptions occurred OnComplete", exception);

            await AbandonInternal(message).ConfigureAwait(false);

            if (errorCallback != null && exception != null)
            {
                await errorCallback(exception).ConfigureAwait(false);
            }
        }

        async Task AbandonInternal(BrokeredMessage message, IDictionary<string, object> propertiesToModify=null)
        {
            if (receiveMode == ReceiveMode.ReceiveAndDelete) return;

            using (var suppressScope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            {
                logger.InfoFormat("Abandoning brokered message {0}", message.MessageId);

                await message.SafeAbandonAsync(propertiesToModify).ConfigureAwait(false);

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

            batchedCompletionCts.Cancel();
            await Task.WhenAll(batchedCompletionTask).ConfigureAwait(false);

            var closeTasks = new List<Task>();
            foreach (var internalReceiver in internalReceivers)
            {
                closeTasks.Add(internalReceiver.CloseAsync());
            }
            await Task.WhenAll(closeTasks).ConfigureAwait(false);

            pipelineInvocationTasks.Clear();

            logger.Info("Notifier for " + fullPath + " stopped");

            isRunning = false;
        }
    }
}