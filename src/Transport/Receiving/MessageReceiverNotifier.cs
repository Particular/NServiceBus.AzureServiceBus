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
        public MessageReceiverNotifier(IManageMessageReceiverLifeCycle clientEntities, IConvertBrokeredMessagesToIncomingMessagesInternal brokeredMessageConverter, ReadOnlySettings settings)
        {
            this.clientEntities = clientEntities;
            this.brokeredMessageConverter = brokeredMessageConverter;
            this.settings = settings;
            RefCount = 1;
        }

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

            internalReceivers = new IMessageReceiver[numberOfClients];
            onMessageOptions = new OnMessageOptions[numberOfClients];
            completion = new MultiProducerConcurrentCompletion<Guid>(1000, TimeSpan.FromSeconds(1), 6, numberOfClients);
        }

        public void Start()
        {
            stopping = false;
            pipelineInvocationTasks = new ConcurrentDictionary<Task, Task>();
            completion.Start(CompletionCallback, internalReceivers);

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

                    var options = new OnMessageOptions
                    {
                        AutoComplete = false,
                        AutoRenewTimeout = autoRenewTimeout,
                        MaxConcurrentCalls = maxConcurrentCalls
                    };
                    options.ExceptionReceived += OptionsOnExceptionReceived;
                    internalReceivers[i] = internalReceiver;
                    onMessageOptions[i] = options;

                    internalReceiver.OnMessage(m => ReceiveMessage(internalReceiver, m, i, pipelineInvocationTasks), options);

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

            foreach (var option in onMessageOptions)
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

            await completion.Complete().ConfigureAwait(false);

            var closeTasks = new List<Task>();
            foreach (var internalReceiver in internalReceivers)
            {
                closeTasks.Add(internalReceiver.CloseAsync());
            }
            await Task.WhenAll(closeTasks).ConfigureAwait(false);

            pipelineInvocationTasks.Clear();
            Array.Clear(internalReceivers, 0, internalReceivers.Length);
            Array.Clear(onMessageOptions, 0, onMessageOptions.Length);

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
                if (stopping)
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

        static Task CompletionCallback(List<Guid> lockTokens, int slotNumber, object state, CancellationToken token)
        {
            var receivers = (IMessageReceiver[]) state;
            var receiver = receivers[slotNumber];
            return receiver.SafeCompleteBatchAsync(lockTokens);
        }

        Task ReceiveMessage(IMessageReceiver internalReceiver, BrokeredMessage message, int slotNumber, ConcurrentDictionary<Task, Task> pipelineInvocations)
        {
            var processTask = ProcessMessage(internalReceiver, message, slotNumber);
            pipelineInvocations.TryAdd(processTask, processTask);
            processTask.ContinueWith((t, state) =>
            {
                var invocations = (ConcurrentDictionary<Task, Task>) state;
                Task toBeRemoved;
                invocations.TryRemove(t, out toBeRemoved);
            }, pipelineInvocations, TaskContinuationOptions.ExecuteSynchronously);
            return processTask;
        }

        async Task ProcessMessage(IMessageReceiver internalReceiver, BrokeredMessage message, int slotNumber)
        {
            if (stopping)
            {
                logger.Info($"Received message with ID {message.MessageId} while shutting down. Message will not be processed and will be retried after {message.LockedUntilUtc}.");
                return;
            }

            try
            {
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

                            await HandleCompletion(message, context, completionCanBeBatched, slotNumber).ConfigureAwait(false);
                            scope?.Complete();
                        }
                    }
                }
                catch (Exception exception) when (!stopping)
                {
                    // and go into recovery mode so that no new messages are added to the transfer queue
                    context.Recovering = true;

                    // pass the context into the error pipeline
                    var transportTransaction = new TransportTransaction();
                    transportTransaction.Set(context);
                    var errorContext = new ErrorContext(exception, incomingMessage.Headers, incomingMessage.MessageId, incomingMessage.Body, transportTransaction, message.DeliveryCount);

                    var result = await processingFailureCallback(errorContext).ConfigureAwait(false);
                    if (result == ErrorHandleResult.RetryRequired)
                    {
                        await Abandon(message, exception).ConfigureAwait(false);
                    }
                    else
                    {
                        await HandleCompletion(message, context, completionCanBeBatched, slotNumber).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception onErrorException)
            {
                await Abandon(message, onErrorException).ConfigureAwait(false);
                await errorCallback(onErrorException).ConfigureAwait(false);
            }
        }

        Task HandleCompletion(BrokeredMessage message, BrokeredMessageReceiveContext context, bool canBeBatched, int slotNumber)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                return AbandonOnCancellation(message);
            }

            if (receiveMode == ReceiveMode.PeekLock)
            {
                if (canBeBatched)
                {
                    completion.Push(message.LockToken, slotNumber);
                }
                else
                {
                    return context.IncomingBrokeredMessage.SafeCompleteAsync();
                }
            }
            return TaskEx.Completed;
        }

        Task AbandonOnCancellation(BrokeredMessage message)
        {
            logger.Debug("Received message is cancelled by the pipeline, abandoning it so we can process it later.");

            return AbandonInternal(message);
        }

        Task Abandon(BrokeredMessage message, Exception exception)
        {
            logger.Debug("Exceptions occurred OnComplete", exception);

            return AbandonInternal(message);
        }

        async Task AbandonInternal(BrokeredMessage message, IDictionary<string, object> propertiesToModify = null)
        {
            if (receiveMode == ReceiveMode.ReceiveAndDelete) return;

            using (var suppressScope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            {
                logger.DebugFormat("Abandoning brokered message {0}", message.MessageId);

                if (await message.SafeAbandonAsync(propertiesToModify).ConfigureAwait(false))
                {
                    logger.DebugFormat("Brokered message {0} abandoned successfully.", message.MessageId);
                }
                else
                {
                    logger.DebugFormat("Abandoning brokered message {0} failed. Message will reappear after peek lock duration is over.", message.MessageId);
                }

                suppressScope.Complete();
            }
        }

        static Task EmptyErrorCallback(Exception exception)
        {
            return TaskEx.Completed;
        }

        IManageMessageReceiverLifeCycle clientEntities;
        IConvertBrokeredMessagesToIncomingMessagesInternal brokeredMessageConverter;
        ReadOnlySettings settings;
        IMessageReceiver[] internalReceivers;
        ReceiveMode receiveMode;
        OnMessageOptions[] onMessageOptions;
        Func<IncomingMessageDetails, ReceiveContext, Task> incomingCallback;
        Func<Exception, Task> errorCallback;
        ConcurrentDictionary<Task, Task> pipelineInvocationTasks;
        string fullPath;
        EntityInfo entity;
        volatile bool stopping;
        volatile bool isRunning;
        Func<ErrorContext, Task<ErrorHandleResult>> processingFailureCallback;
        int numberOfClients;
        MultiProducerConcurrentCompletion<Guid> completion;
        int maxConcurrentCalls;
        TimeSpan autoRenewTimeout;
        bool wrapInScope;
        bool completionCanBeBatched;
        static ILog logger = LogManager.GetLogger<MessageReceiverNotifier>();
    }
}