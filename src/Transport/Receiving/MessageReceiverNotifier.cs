namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using Microsoft.ServiceBus.Messaging;
    using Logging;
    using Azure.Transports.WindowsAzureServiceBus;
    using Settings;
    using Transport;
    using Transports;
    using Utils;

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

        public bool IsRunning { get; private set; }
        public int RefCount { get; set; }

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

            PerformBatchedCompletionTask();
        }

        void PerformBatchedCompletionTask()
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
                    await internalReceiver.SafeCompleteBatchAsync(buffer.Take(count)).ConfigureAwait(false);
                    Array.Clear(buffer, 0, buffer.Length);
                }

                // run last check when task has been cancelled to drain remaining lock tokens
                if (locksTokensToComplete.IsEmpty)
                {
                    return;
                }

                count = locksTokensToComplete.TryPopRange(buffer);
                await internalReceiver.SafeCompleteBatchAsync(buffer.Take(count)).ConfigureAwait(false);
                Array.Clear(buffer, 0, buffer.Length);

            }, CancellationToken.None);
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
                // Messages that have been handled by recovery, but abandoned to clear via queue can now be safely completed
                if (message.Properties.ContainsKey(BrokeredMessageHeaders.Recovery))
                {
                    locksTokensToComplete.Push(message.LockToken);
                    return; // do not invoke the pipeline
                }
            }
            catch (UnsupportedBrokeredMessageBodyTypeException exception)
            {
                await message.DeadLetterAsync("BrokeredMessage to IncomingMessageDetails conversion failure", exception.ToString()).ConfigureAwait(false);
                return;
            }

            var context = new BrokeredMessageReceiveContext(message, entity, internalReceiver.Mode);

            try
            {
                await incomingCallback(incomingMessage, context).ConfigureAwait(false);

                if (context.CancellationToken.IsCancellationRequested)
                {
                    await AbandonAsyncOnCancellation(message).ConfigureAwait(false);
                }
                else
                {
                    if (context.ReceiveMode == ReceiveMode.PeekLock)
                    {
                        locksTokensToComplete.Push(message.LockToken);
                    }
                }
            }
            catch (Exception exception)
            {
                var transportTransaction = new TransportTransaction();
                context.Recovering = true;
                transportTransaction.Set(context);
                incomingMessage.BodyStream.Seek(0, SeekOrigin.Begin);
                var errorContext = new ErrorContext(exception, incomingMessage.Headers, incomingMessage.MessageId, incomingMessage.BodyStream, transportTransaction, message.DeliveryCount);

                try
                {
                    var result = await processingFailureCallback(errorContext).ConfigureAwait(false);
                    if (result == ErrorHandleResult.RetryRequired)
                    {
                        await AbandonAsync(message, exception).ConfigureAwait(false);
                    }
                    else if (settings.Get<bool>(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue))
                    {
                        var recoveryHeader = new Dictionary<string, object> { { BrokeredMessageHeaders.Recovery, "Message failed " + message.DeliveryCount + " times and was handled by the nservicebus recovery infrastructure. Safe to remove it." } };
                        await AbandonInternal(message, recoveryHeader).ConfigureAwait(false);
                    }
                    else if (result == ErrorHandleResult.Handled && context.ReceiveMode == ReceiveMode.PeekLock)
                    {
                        locksTokensToComplete.Push(message.LockToken);
                    }
                }
                catch (Exception onErrorException)
                {
                    await AbandonAsync(message, onErrorException).ConfigureAwait(false);
                }
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

            pipelineInvocationTasks.Clear();

            await internalReceiver.CloseAsync().ConfigureAwait(false);

            logger.Info("Notifier for " + fullPath + " stopped");

            IsRunning = false;
        }
    }
}