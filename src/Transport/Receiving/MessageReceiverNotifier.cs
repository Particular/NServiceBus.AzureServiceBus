namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using Logging;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus;
    using Settings;

    class MessageReceiverNotifier : INotifyIncomingMessages
    {
        public MessageReceiverNotifier(IManageMessageReceiverLifeCycle clientEntities, IConvertBrokeredMessagesToIncomingMessages brokeredMessageConverter, ReadOnlySettings settings)
        {
            this.clientEntities = clientEntities;
            this.brokeredMessageConverter = brokeredMessageConverter;
            this.settings = settings;
            locksTokensToComplete = new ConcurrentStack<Guid>();
            batchedCompletionCts = new CancellationTokenSource();
            RefCount = 1;
        }

        bool ShouldReceiveMessages => !stopping;

        public bool IsRunning => isRunning;

        public int RefCount { get; set; }

        public void Initialize(EntityInfo entity, Func<IncomingMessageDetails, ReceiveContext, Task> callback, Func<Exception, Task> errorCallback, Func<ErrorContext, Task<ErrorHandleResult>> processingFailureCallback, int maximumConcurrency)
        {
            receiveMode = settings.Get<ReceiveMode>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode);

            incomingCallback = callback;
            this.errorCallback = errorCallback ?? EmptyErrorCallback;
            this.processingFailureCallback = processingFailureCallback;
            this.entity = entity;

            fullPath = entity.Path;
            if (entity.Type == EntityType.Subscription)
            {
                var topic = entity.RelationShips.First(r => r.Type == EntityRelationShipType.Subscription);
                fullPath = SubscriptionClient.FormatSubscriptionPath(topic.Target.Path, entity.Path);
            }

            var transportTransactionMode = settings.HasExplicitValue<TransportTransactionMode>() ? settings.Get<TransportTransactionMode>() : settings.SupportedTransactionMode();
            wrapInScope = transportTransactionMode == TransportTransactionMode.SendsAtomicWithReceive;
            completionCanBeBatched = !wrapInScope;
            autoRenewTimeout = settings.Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.AutoRenewTimeout);
            numberOfClients = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity);
            var concurrency = maximumConcurrency/(double) numberOfClients;
            maxConcurrentCalls = concurrency > 1 ? (int) Math.Round(concurrency, MidpointRounding.AwayFromZero) : 1;
            if (Math.Abs(maxConcurrentCalls - concurrency) > 0)
            {
                logger.InfoFormat("The maximum concurrency on this message receiver instance has been adjusted to '{0}', because the total maximum concurrency '{1}' wasn't divisable by the number of clients '{2}'", maxConcurrentCalls, maximumConcurrency, numberOfClients);
            }

            batchedCompletionTasks = new Task[numberOfClients];
            internalReceivers = new IMessageReceiver[numberOfClients];
            options = new OnMessageOptions[numberOfClients];
        }

        public void Start()
        {
            stopping = false;
            pipelineInvocationTasks = new ConcurrentDictionary<Task, Task>();

            var exceptions = new ConcurrentQueue<Exception>();
            Parallel.For(0, numberOfClients, i =>
            {
                try
                {
                    var internalReceiver = clientEntities.Get(fullPath, entity.Namespace.Alias);

                    if (internalReceiver == null)
                    {
                        throw new Exception($"MessageReceiverNotifier did not get a MessageReceiver instance for entity path {fullPath}, this is probably due to a misconfiguration of the topology");
                    }

                    var onMessageOptions = new OnMessageOptions
                    {
                        AutoComplete = false,
                        AutoRenewTimeout = autoRenewTimeout,
                        MaxConcurrentCalls = maxConcurrentCalls
                    };
                    onMessageOptions.ExceptionReceived += OptionsOnExceptionReceived;
                    internalReceiver.OnMessage(m => ReceiveMessage(internalReceiver, m, pipelineInvocationTasks), onMessageOptions);
                    PerformBatchedCompletionTask(internalReceiver, i);

                    internalReceivers[i] = internalReceiver;
                    options[i] = onMessageOptions;

                    isRunning = true;
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            });
            if (exceptions.Count > 0) throw new AggregateException(exceptions);
        }

        public async Task Stop()
        {
            stopping = true;

            logger.Info($"Stopping notifier for '{fullPath}'");

            foreach (var option in options)
            {
                option.ExceptionReceived -= OptionsOnExceptionReceived;
            }

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
            var allTasks = pipelineInvocationTasks.Values;
            var finishedTask = await Task.WhenAny(Task.WhenAll(allTasks), timeoutTask).ConfigureAwait(false);

            if (finishedTask.Equals(timeoutTask))
            {
                logger.Error("The receiver failed to stop with in the time allowed (30s)");
            }

            batchedCompletionCts.Cancel();
            await Task.WhenAll(batchedCompletionTasks).ConfigureAwait(false);

            var closeTasks = new List<Task>();
            foreach (var internalReceiver in internalReceivers)
            {
                closeTasks.Add(internalReceiver.CloseAsync());
            }
            await Task.WhenAll(closeTasks).ConfigureAwait(false);

            pipelineInvocationTasks.Clear();
            Array.Clear(batchedCompletionTasks, 0, batchedCompletionTasks.Length);
            Array.Clear(internalReceivers, 0, internalReceivers.Length);
            Array.Clear(options, 0, options.Length);

            logger.Info($"Notifier for '{fullPath}' stopped");

            isRunning = false;
        }

        // Intentionally made async void since we don't care about the outcome here
        // according to blog posts, this method is invoked on
        //- Exceptions raised during the time that the message pump is waiting for messages to arrive on the service bus
        //- Exceptions raised during the time that your code is processing the BrokeredMessage
        //- It is raised when the receive process successfully completes. (Does not seem to be the case)
        async void OptionsOnExceptionReceived(object sender, ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            try
            {
                if (!ShouldReceiveMessages)
                {
                    logger.Info($"OptionsOnExceptionReceived invoked, action: '{exceptionReceivedEventArgs.Action}' while shutting down.");
                    return;
                }

                //- It is raised when the underlying connection closes because of our close operation
                var messagingException = exceptionReceivedEventArgs.Exception as MessagingException;
                if (messagingException != null && messagingException.IsTransient)
                {
                    logger.DebugFormat("OptionsOnExceptionReceived invoked, action: '{0}', transient exception with message: {1}", exceptionReceivedEventArgs.Action, messagingException.Detail.Message);
                }
                else
                {
                    logger.Info($"OptionsOnExceptionReceived invoked, action: '{exceptionReceivedEventArgs.Action}', with non-transient exception.", exceptionReceivedEventArgs.Exception);

                    await errorCallback.Invoke(exceptionReceivedEventArgs.Exception).ConfigureAwait(false);
                }
            }
            catch 
            {
                // Intentionally left blank. Any exception raised to the SDK would issue an Environment.FailFast
            }
        }

        Task ReceiveMessage(IMessageReceiver internalReceiver, BrokeredMessage message, ConcurrentDictionary<Task, Task> pipelineInvocations)
        {
            var processTask = ProcessMessage(internalReceiver, message);
            pipelineInvocations.TryAdd(processTask, processTask);
            processTask.ContinueWith((t, state) =>
            {
                var invocations = (ConcurrentDictionary<Task, Task>) state;
                Task toBeRemoved;
                invocations.TryRemove(t, out toBeRemoved);
            }, pipelineInvocations, TaskContinuationOptions.ExecuteSynchronously);
            return processTask;
        }

        void PerformBatchedCompletionTask(IMessageReceiver internalReceiver, int index)
        {
            batchedCompletionTasks[index] = Task.Run(async () =>
            {
                var buffer = new Guid[5000];

                while (!batchedCompletionCts.Token.IsCancellationRequested || !locksTokensToComplete.IsEmpty)
                {
                    // running concurrently, two tasks could get to this line, but only one will pop items
                    var count = locksTokensToComplete.TryPopRange(buffer, 0, buffer.Length);

                    if (count == 0)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
                        continue;
                    }

                    var toComplete = buffer.Take(count).ToList();
                    await internalReceiver.SafeCompleteBatchAsync(toComplete).ConfigureAwait(false);
                }
            }, CancellationToken.None);
        }

        async Task ProcessMessage(IMessageReceiver internalReceiver, BrokeredMessage message)
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
            try
            {
                var scope = wrapInScope ? new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.Serializable
                }, TransactionScopeAsyncFlowOption.Enabled) : null;
                {
                    using (scope)
                    {
                        await incomingCallback(incomingMessage, context).ConfigureAwait(false);

                        await HandleCompletion(message, context, completionCanBeBatched).ConfigureAwait(false);
                        scope?.Complete();
                    }
                }
            }
            catch (Exception exception) when (ShouldReceiveMessages)
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

            if (exception != null)
            {
                await errorCallback(exception).ConfigureAwait(false);
            }
        }

        async Task AbandonInternal(BrokeredMessage message, IDictionary<string, object> propertiesToModify = null)
        {
            if (receiveMode == ReceiveMode.ReceiveAndDelete) return;

            using (var suppressScope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            {
                logger.InfoFormat("Abandoning brokered message {0}", message.MessageId);

                await message.SafeAbandonAsync(propertiesToModify).ConfigureAwait(false);

                suppressScope.Complete();
            }
        }

        static Task EmptyErrorCallback(Exception exception)
        {
            return TaskEx.Completed;
        }

        IManageMessageReceiverLifeCycle clientEntities;

        IConvertBrokeredMessagesToIncomingMessages brokeredMessageConverter;
        ReadOnlySettings settings;
        IMessageReceiver[] internalReceivers;
        ReceiveMode receiveMode;
        OnMessageOptions[] options;
        Func<IncomingMessageDetails, ReceiveContext, Task> incomingCallback;
        Func<Exception, Task> errorCallback;
        ConcurrentDictionary<Task, Task> pipelineInvocationTasks;
        string fullPath;
        EntityInfo entity;
        volatile bool stopping;
        volatile bool isRunning;
        ConcurrentStack<Guid> locksTokensToComplete;
        CancellationTokenSource batchedCompletionCts;
        Task[] batchedCompletionTasks;
        Func<ErrorContext, Task<ErrorHandleResult>> processingFailureCallback;
        int numberOfClients;
        int maxConcurrentCalls;
        TimeSpan autoRenewTimeout;
        static ILog logger = LogManager.GetLogger<MessageReceiverNotifier>();
        bool wrapInScope;
        bool completionCanBeBatched;
    }
}