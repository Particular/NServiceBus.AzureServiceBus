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
    using NServiceBus.AzureServiceBus;
    using Settings;
    using Transport;

    class MessageReceiverNotifier : INotifyIncomingMessages
    {
        IManageMessageReceiverLifeCycle clientEntities;
        IConvertBrokeredMessagesToIncomingMessages brokeredMessageConverter;
        ReadOnlySettings settings;
        IMessageReceiver[] internalReceivers;
        ReceiveMode receiveMode;
        OnMessageOptions options;
        Func<IncomingMessageDetails, ReceiveContext, Task> incomingCallback;
        Func<Exception, Task> errorCallback;
        ConcurrentDictionary<Task, Task> pipelineInvocationTasks;
        string fullPath;
        EntityInfo entity;
        volatile bool stopping;
        volatile bool isRunning;
        static ILog logger = LogManager.GetLogger<MessageReceiverNotifier>();
        Func<ErrorContext, Task<ErrorHandleResult>> processingFailureCallback;
        int numberOfClients;
        MultiProducerConcurrentCompletion<Guid> completion;

        public MessageReceiverNotifier(IManageMessageReceiverLifeCycle clientEntities, IConvertBrokeredMessagesToIncomingMessages brokeredMessageConverter, ReadOnlySettings settings)
        {
            this.clientEntities = clientEntities;
            this.brokeredMessageConverter = brokeredMessageConverter;
            this.settings = settings;
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

            numberOfClients = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity);
            var concurrency = maximumConcurrency / (double)numberOfClients;
            var maxConcurrentCalls = concurrency > 1 ? (int)Math.Round(concurrency, MidpointRounding.AwayFromZero) : 1;
            if (Math.Abs(maxConcurrentCalls - concurrency) > 0)
            {
                logger.InfoFormat("The maximum concurrency on this message receiver instance has been adjusted to '{0}', because the total maximum concurrency '{1}' wasn't divisable by the number of clients '{2}'", maxConcurrentCalls, maximumConcurrency, numberOfClients);
            }
            options = new OnMessageOptions
            {
                AutoComplete = false,
                AutoRenewTimeout = settings.Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.AutoRenewTimeout),
                MaxConcurrentCalls = maxConcurrentCalls
            };

            options.ExceptionReceived += OptionsOnExceptionReceived;

            internalReceivers = new IMessageReceiver[numberOfClients];
            completion = new MultiProducerConcurrentCompletion<Guid>(1000, TimeSpan.FromSeconds(1), 6, numberOfClients);
        }

        void OptionsOnExceptionReceived(object sender, ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            // according to blog posts, this method is invoked on
            //- Exceptions raised during the time that the message pump is waiting for messages to arrive on the service bus
            //- Exceptions raised during the time that your code is processing the BrokeredMessage
            //- It is raised when the receive process successfully completes. (Does not seem to be the case)

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

                // TODO ideally we'd failover to another space if in a certain period of time there are too many transient errors
            }
            else
            {
                logger.Info($"OptionsOnExceptionReceived invoked, action: '{exceptionReceivedEventArgs.Action}', with non-transient exception.", exceptionReceivedEventArgs.Exception);

                errorCallback?.Invoke(exceptionReceivedEventArgs.Exception).GetAwaiter().GetResult();
            }
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

                    internalReceiver.OnMessage(m => ReceiveMessage(internalReceiver, m, i, pipelineInvocationTasks), options);
                    internalReceivers[i] = internalReceiver;

                    isRunning = true;
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            });
            if (exceptions.Count > 0) throw new AggregateException(exceptions);
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

                        await HandleCompletion(message, context, completionCanBeBatched, slotNumber).ConfigureAwait(false);
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
                        await Abandon(message, exception).ConfigureAwait(false);
                    }
                    else
                    {
                        await HandleCompletion(message, context, completionCanBeBatched, slotNumber).ConfigureAwait(false);
                    }
                }
                catch (Exception onErrorException)
                {
                    await Abandon(message, onErrorException).ConfigureAwait(false);
                }
            }
        }

        async Task HandleCompletion(BrokeredMessage message, BrokeredMessageReceiveContext context, bool canbeBatched, int slotNumber)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                await AbandonOnCancellation(message).ConfigureAwait(false);
            }
            else
            {
                if (receiveMode == ReceiveMode.PeekLock)
                {
                    if (canbeBatched)
                    {
                        completion.Push(message.LockToken, slotNumber);
                    }
                    else
                    {
                        await context.IncomingBrokeredMessage.CompleteAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        Task AbandonOnCancellation(BrokeredMessage message)
        {
            logger.Info("Received message is cancelled by the pipeline, abandoning it so we can process it later.");

            return AbandonInternal(message);
        }

        async Task Abandon(BrokeredMessage message, Exception exception)
        {
            logger.Info("Exceptions occurred OnComplete", exception);

            await AbandonInternal(message).ConfigureAwait(false);

            if (errorCallback != null && exception != null)
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

        public async Task Stop()
        {
            options.ExceptionReceived -= OptionsOnExceptionReceived;

            stopping = true;

            logger.Info($"Stopping notifier for '{fullPath}'");

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

            logger.Info($"Notifier for '{fullPath}' stopped");

            isRunning = false;
        }
    }
}