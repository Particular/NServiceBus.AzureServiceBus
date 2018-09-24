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
    using NServiceBus.AzureServiceBus.Utils;

    class MessageReceiverNotifier : INotifyIncomingMessagesInternal
    {
        public MessageReceiverNotifier(MessageReceiverCreator receiverCreator, BrokeredMessagesToIncomingMessagesConverter brokeredMessageConverter, MessageReceiverNotifierSettings settings)
        {
            messageReceiverCreator = receiverCreator;
            this.brokeredMessageConverter = brokeredMessageConverter;
            this.settings = settings;
        }

        public void Initialize(EntityInfoInternal entity, Func<IncomingMessageDetailsInternal, ReceiveContextInternal, Task> callback, Func<Exception, Task> errorCallback, Action<Exception> criticalError, Func<ErrorContext, Task<ErrorHandleResult>> processingFailureCallback, int maximumConcurrency)
        {
            incomingCallback = callback;
            this.criticalError = criticalError;
            this.errorCallback = errorCallback ?? EmptyErrorCallback;
            this.processingFailureCallback = processingFailureCallback;
            this.entity = entity;

            fullPath = entity.Path;
            if (entity.Type == EntityType.Subscription)
            {
                var topic = entity.RelationShips.First(r => r.Type == EntityRelationShipTypeInternal.Subscription);
                fullPath = SubscriptionClient.FormatSubscriptionPath(topic.Target.Path, entity.Path);
            }

            wrapInScope = settings.TransportTransactionMode == TransportTransactionMode.SendsAtomicWithReceive;
            // batching will be applied for transaction modes other than SendsAtomicWithReceive
            completionCanBeBatched = !wrapInScope;

            var numberOfClients = settings.NumberOfClients;
            var concurrency = maximumConcurrency / (double)numberOfClients;
            maxConcurrentCalls = concurrency > 1 ? (int)Math.Round(concurrency, MidpointRounding.AwayFromZero) : 1;
            if (Math.Abs(maxConcurrentCalls - concurrency) > 0)
            {
                logger.InfoFormat("The maximum concurrency on message receiver instance for '{0}' has been adjusted to '{1}', because the total maximum concurrency '{2}' wasn't divisible by the number of clients '{3}'", fullPath, maxConcurrentCalls, maximumConcurrency, numberOfClients);
            }

            internalReceivers = new IMessageReceiverInternal[numberOfClients];
            onMessageOptions = new OnMessageOptions[numberOfClients];

            // when we don't batch we don't need the completion infrastructure
            completion = completionCanBeBatched ? new MultiProducerConcurrentCompletion<Guid>(1000, TimeSpan.FromSeconds(1), 6, numberOfClients) : null;
        }

        public void Start()
        {
            if (Interlocked.Increment(ref refCount) == 1)
            {
                StartInternal();
            }
        }

        void StartInternal()
        {
            stopping = false;
            pipelineInvocationTasks = new ConcurrentDictionary<Task, Task>();
            completion?.Start(CompletionCallback, internalReceivers);

            // Offloading here to make sure calling thread is not used for sync path inside
            // async call stack.
            startTask = Task.Run(async () =>
            {
                var exceptions = new ConcurrentQueue<Exception>();
                var tasks = new Task[settings.NumberOfClients];
                for (var i = 0; i < settings.NumberOfClients; i++)
                {
                    tasks[i] = InitializeAndStartReceiver(i, exceptions);
                }

                await Task.WhenAll(tasks)
                    .ConfigureAwait(false);

                if (!exceptions.IsEmpty)
                {
                    criticalError(new AggregateException(exceptions));
                }
            });
        }

        public async Task Stop()
        {
            if (Interlocked.Decrement(ref refCount) == 0)
            {
                await StopInternal().ConfigureAwait(false);
            }
        }

        async Task StopInternal()
        {
            stopping = true;

            logger.Info($"Stopping notifier for '{fullPath}'");

            await startTask.ConfigureAwait(false);

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

            if (completion != null)
            {
                await completion.Complete().ConfigureAwait(false);
            }

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
        }

        async Task InitializeAndStartReceiver(int i, ConcurrentQueue<Exception> exceptions)
        {
            var attempt = 0;
            do
            {
                try
                {
                    var internalReceiver = await messageReceiverCreator.Create(fullPath, entity.Namespace.Alias)
                        .ConfigureAwait(false);

                    var options = new OnMessageOptions
                    {
                        AutoComplete = false,
                        AutoRenewTimeout = settings.AutoRenewTimeout,
                        MaxConcurrentCalls = maxConcurrentCalls
                    };
                    options.ExceptionReceived += OptionsOnExceptionReceived;
                    internalReceivers[i] = internalReceiver ?? throw new Exception($"MessageReceiverNotifier did not get a MessageReceiver instance for entity path {fullPath}, this is probably due to a misconfiguration of the topology");
                    onMessageOptions[i] = options;

                    if (internalReceiver.IsClosed)
                    {
                        throw new Exception($"MessageReceiverNotifier did not get an open MessageReceiver instance for entity path {fullPath}");
                    }
                    internalReceiver.OnMessage(m => ReceiveMessage(internalReceiver, m, i, pipelineInvocationTasks), options);
                    return;
                }
                catch (Exception ex)
                {
                    attempt++;
                    if (attempt == 2)
                    {
                        exceptions.Enqueue(ex);
                    }
                }
            } while (attempt < 2);
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

                if (exceptionReceivedEventArgs.Exception.IsTransientException())
                {
                    logger.DebugFormat("OptionsOnExceptionReceived invoked, action: '{0}', transient exception with message: {1}", exceptionReceivedEventArgs.Action, exceptionReceivedEventArgs.Exception.Message);
                }
                else
                {
                    logger.Info($"OptionsOnExceptionReceived invoked, action: '{exceptionReceivedEventArgs.Action}', with non-transient exception.", exceptionReceivedEventArgs.Exception);

                    await errorCallback(exceptionReceivedEventArgs.Exception).ConfigureAwait(false);
                }
            }
            catch
            {
                // Intentionally left blank. Any exception raised to the SDK would issue an Environment.FailFast
            }
        }

        static Task CompletionCallback(List<Guid> lockTokens, int slotNumber, object state, CancellationToken token)
        {
            var receivers = (IMessageReceiverInternal[])state;
            var receiver = receivers[slotNumber];
            return receiver.SafeCompleteBatchAsync(lockTokens);
        }

        Task ReceiveMessage(IMessageReceiverInternal internalReceiver, BrokeredMessage message, int slotNumber, ConcurrentDictionary<Task, Task> pipelineInvocations)
        {
            var processTask = ProcessMessage(internalReceiver, message, slotNumber);
            pipelineInvocations.TryAdd(processTask, processTask);
            processTask.ContinueWith((t, state) =>
            {
                var invocations = (ConcurrentDictionary<Task, Task>)state;
                invocations.TryRemove(t, out var _);
            }, pipelineInvocations, TaskContinuationOptions.ExecuteSynchronously).Ignore();
            return processTask;
        }

        async Task ProcessMessage(IMessageReceiverInternal internalReceiver, BrokeredMessage message, int slotNumber)
        {
            if (stopping)
            {
                logger.Info($"Received message with ID {message.MessageId} while shutting down. Message will not be processed and will be retried after {message.LockedUntilUtc}.");
                return;
            }

            try
            {
                IncomingMessageDetailsInternal incomingMessage;
                try
                {
                    incomingMessage = brokeredMessageConverter.Convert(message);
                }
                catch (UnsupportedBrokeredMessageBodyTypeException exception)
                {
                    await message.DeadLetterAsync("BrokeredMessage to IncomingMessageDetails conversion failure", exception.ToString()).ConfigureAwait(false);
                    return;
                }

                var context = new BrokeredMessageReceiveContextInternal(message, entity, internalReceiver.Mode);

                    var scope = wrapInScope
                        ? new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions
                        {
                            IsolationLevel = IsolationLevel.Serializable
                        }, TransactionScopeAsyncFlowOption.Enabled)
                        : null;
                    {
                        using (scope)
                        {
                            var wasCompleted = false;
                            try
                            {
                                await incomingCallback(incomingMessage, context).ConfigureAwait(false);

                                if (context.CancellationToken.IsCancellationRequested)
                                {
                                    await AbandonOnCancellation(message).ConfigureAwait(false);
                                }
                                else
                                {
                                    wasCompleted = await HandleCompletion(message, context, completionCanBeBatched, slotNumber).ConfigureAwait(false);
                                }
                            }
                            catch (Exception exception) when (!stopping)
                            {
                                // and go into recovery mode so that no new messages are added to the transfer queue
                                context.Recovering = true;

                                // pass the context into the error pipeline
                                var transportTransaction = new TransportTransaction();
                                transportTransaction.Set(context);
                                var errorContext = new ErrorContext(exception, brokeredMessageConverter.GetHeaders(message), incomingMessage.MessageId, incomingMessage.Body, transportTransaction, message.DeliveryCount);

                                var result = await processingFailureCallback(errorContext).ConfigureAwait(false);
                                if (result == ErrorHandleResult.RetryRequired)
                                {
                                    await Abandon(message, exception).ConfigureAwait(false);
                                }
                                else
                                {
                                    wasCompleted = await HandleCompletion(message, context, completionCanBeBatched, slotNumber).ConfigureAwait(false);
                                }
                            }
                            finally
                            {
                                if (wasCompleted)
                                {
                                    scope?.Complete();
                                }
                            }
                        }
                    }

            }
            catch (Exception onErrorException)
            {
                await Abandon(message, onErrorException).ConfigureAwait(false);
                await errorCallback(onErrorException).ConfigureAwait(false);
            }
        }

        Task<bool> HandleCompletion(BrokeredMessage message, BrokeredMessageReceiveContextInternal context, bool canBeBatched, int slotNumber)
        {
            if (settings.ReceiveMode == ReceiveMode.PeekLock)
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
            return TaskEx.CompletedTrue;
        }

        Task<bool> AbandonOnCancellation(BrokeredMessage message)
        {
            logger.Debug("Received message is canceled by the pipeline, abandoning it so we can process it later.");

            return AbandonInternal(message);
        }

        Task<bool> Abandon(BrokeredMessage message, Exception exception)
        {
            logger.Debug("Exceptions occurred OnComplete", exception);

            return AbandonInternal(message);
        }

        async Task<bool> AbandonInternal(BrokeredMessage message, IDictionary<string, object> propertiesToModify = null)
        {
            if (settings.ReceiveMode == ReceiveMode.ReceiveAndDelete)
            {
                return true;
            }

            using (var suppressScope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            {
                logger.DebugFormat("Abandoning brokered message {0}", message.MessageId);

                var wasAbandoned = await message.SafeAbandonAsync(propertiesToModify).ConfigureAwait(false);
                if (wasAbandoned)
                {
                    logger.DebugFormat("Brokered message {0} abandoned successfully.", message.MessageId);
                }
                else
                {
                    logger.DebugFormat("Abandoning brokered message {0} failed. Message will reappear after peek lock duration is over.", message.MessageId);
                }

                suppressScope.Complete();

                return wasAbandoned;
            }
        }

        static Task EmptyErrorCallback(Exception exception)
        {
            return TaskEx.Completed;
        }

        BrokeredMessagesToIncomingMessagesConverter brokeredMessageConverter;
        IMessageReceiverInternal[] internalReceivers;
        OnMessageOptions[] onMessageOptions;
        Func<IncomingMessageDetailsInternal, ReceiveContextInternal, Task> incomingCallback;
        Func<Exception, Task> errorCallback;
        ConcurrentDictionary<Task, Task> pipelineInvocationTasks;
        string fullPath;
        EntityInfoInternal entity;
        long refCount;
        volatile bool stopping;
        Func<ErrorContext, Task<ErrorHandleResult>> processingFailureCallback;
        MultiProducerConcurrentCompletion<Guid> completion;
        int maxConcurrentCalls;
        bool wrapInScope;
        bool completionCanBeBatched;
        MessageReceiverNotifierSettings settings;
        static ILog logger = LogManager.GetLogger<MessageReceiverNotifier>();
        Task startTask;
        Action<Exception> criticalError;
        readonly MessageReceiverCreator messageReceiverCreator;
    }
}
