namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Transactions;
    using Microsoft.ServiceBus.Messaging;
    using System.Threading;
    using System.Threading.Tasks;
    using CircuitBreakers;
    using NServiceBus.Logging;
    using NServiceBus.Transports;
    using Unicast.Transport;

    /// <summary>
    /// Azure service bus implementation if <see cref="IDequeueMessages" />.
    /// </summary>
    class AzureServiceBusDequeueStrategy : IDequeueMessages
    {
        ITopology topology;
        readonly CriticalError criticalError;
        Address address;
        TransactionSettings settings;
        Func<TransportMessage, bool> tryProcessMessage;
        Action<TransportMessage, Exception> endProcessMessage;
        TransactionOptions transactionOptions;
        BlockingCollection<BrokeredMessage> pendingMessages;

        ConcurrentDictionary<string, INotifyReceivedBrokeredMessages> notifiers = new ConcurrentDictionary<string, INotifyReceivedBrokeredMessages>();
        CancellationTokenSource tokenSource;
        RepeatedFailuresOverTimeCircuitBreaker circuitBreaker;

        ILog logger = LogManager.GetLogger(typeof(AzureServiceBusDequeueStrategy));

        public AzureServiceBusDequeueStrategy(ITopology topology, CriticalError criticalError)
        {
            this.topology = topology;
            this.criticalError = criticalError;
            circuitBreaker = new RepeatedFailuresOverTimeCircuitBreaker("AzureStoragePollingDequeueStrategy", TimeSpan.FromSeconds(30), ex => criticalError.Raise("Failed to receive message from Azure ServiceBus.", ex));
        }

        public int BatchSize { get; set; }

        /// <summary>
        /// Initializes the <see cref="IDequeueMessages"/>.
        /// </summary>
        /// <param name="address">The address to listen on.</param>
        /// <param name="transactionSettings">The <see cref="TransactionSettings"/> to be used by <see cref="IDequeueMessages"/>.</param>
        /// <param name="tryProcessMessage">Called when a message has been dequeued and is ready for processing.</param>
        /// <param name="endProcessMessage">Needs to be called by <see cref="IDequeueMessages"/> after the message has been processed regardless if the outcome was successful or not.</param>
        public virtual void Init(Address address, TransactionSettings transactionSettings, Func<TransportMessage, bool> tryProcessMessage, Action<TransportMessage, Exception> endProcessMessage)
        {
            settings = transactionSettings;
            this.tryProcessMessage = tryProcessMessage;
            this.endProcessMessage = endProcessMessage;
            this.address = address;

            transactionOptions = new TransactionOptions { IsolationLevel = transactionSettings.IsolationLevel, Timeout = transactionSettings.TransactionTimeout };
        }

        /// <summary>
        /// Starts the dequeuing of message using the specified <paramref name="maximumConcurrencyLevel"/>.
        /// </summary>
        /// <param name="maximumConcurrencyLevel">Indicates the maximum concurrency level this <see cref="IDequeueMessages"/> is able to support.</param>
        public virtual void Start(int maximumConcurrencyLevel)
        {
            pendingMessages = new BlockingCollection<BrokeredMessage>(BatchSize * maximumConcurrencyLevel);

            CreateAndTrackNotifier();

            tokenSource = new CancellationTokenSource();

            for (var i = 0; i < maximumConcurrencyLevel; i++)
            {
                StartThread();
            }
        }

        void StartThread()
        {
            var token = tokenSource.Token;

            Task.Factory
                .StartNew(TryProcessMessage, token, token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        t.Exception.Handle(ex =>
                        {
                            circuitBreaker.Failure(ex);
                            return true;
                        });
                    }

                    StartThread();
                }, TaskContinuationOptions.OnlyOnFaulted);
        }


        void TryProcessMessage(object obj)
        {
            var cancellationToken = (CancellationToken) obj;

            while (!cancellationToken.IsCancellationRequested)
            {
                var brokeredMessage = pendingMessages.Take(cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                Exception exception = null;

                // due to clock drift we may receive messages that aren't due yet according to our clock, let's put this back
                if (brokeredMessage.ScheduledEnqueueTimeUtc > DateTime.UtcNow)
                {
                    pendingMessages.Add(brokeredMessage, cancellationToken);
                    continue;
                }


                if (!RenewLockIfNeeded(brokeredMessage)) continue;

                var transportMessage = brokeredMessage.ToTransportMessage();

                try
                {
                    if (settings.IsTransactional)
                    {
                        using (var scope = new TransactionScope(TransactionScopeOption.Required, transactionOptions))
                        {
                            Transaction.Current.EnlistVolatile(new ReceiveResourceManager(brokeredMessage), EnlistmentOptions.None);

                            if (transportMessage != null)
                            {
                                if (tryProcessMessage(transportMessage))
                                {
                                    scope.Complete();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (transportMessage != null)
                        {
                            tryProcessMessage(transportMessage);
                        }
                    }

                    circuitBreaker.Success();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    if (!cancellationToken.IsCancellationRequested && (transportMessage != null || exception != null))
                    {
                        endProcessMessage(transportMessage, exception);
                    }
                }
            }
        }

        static bool RenewLockIfNeeded(BrokeredMessage brokeredMessage)
        {
            try
            {
                if (brokeredMessage.LockedUntilUtc <= DateTime.UtcNow) return false;

                if (brokeredMessage.LockedUntilUtc <= DateTime.UtcNow.AddSeconds(10))
                {
                    try
                    {
                        brokeredMessage.RenewLock();
                    }
                    catch (MessageLockLostException)
                    {
                        return false;
                    }
                    catch (SessionLockLostException)
                    {
                        return false;
                    }
                    catch (TimeoutException)
                    {
                        return false;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // if the message was received without a peeklock mechanism you're not allowed to call LockedUntilUtc
                // sadly enough I can't find a public property that checks who the receiver was or if the locktoken has been set
                // those are internal to the sdk
            }

            return true;
        }

        /// <summary>
        ///     Stops the dequeuing of messages.
        /// </summary>
        public virtual void Stop()
        {
            foreach (var notifier in notifiers.Values)
            {
                notifier.Stop();
            }

            notifiers.Clear();

            tokenSource.Cancel();
        }

        void CreateAndTrackNotifier()
        {
            logger.InfoFormat("Creating a new notifier for address {0}", address.ToString());

            var notifier = topology.GetReceiver(address);

            notifier.Faulted += NotifierFaulted;

            TrackNotifier(null, address, notifier);
        }

        void NotifierFaulted(object sender, EventArgs e)
        {
            RemoveNotifier(null, address);
            CreateAndTrackNotifier();
        }

        public void TrackNotifier(Type eventType, Address original, INotifyReceivedBrokeredMessages notifier)
        {
            var key = CreateKeyFor(eventType, original);

            notifier.Start(EnqueueMessage, ErrorDequeueingBatch);
            notifiers.AddOrUpdate(key, notifier, (s, n) => notifier);

            if (eventType != null)
            {
                logger.InfoFormat("Started tracking new notifier for event type {0}, address {1}",  eventType.Name, original.ToString());
            }
            else
            {
                logger.InfoFormat("Started tracking new notifier for address {0}", original.ToString());
            }
        }

        public void RemoveNotifier(Type eventType, Address original)
        {
            var key = CreateKeyFor(eventType, original);
            if (!notifiers.ContainsKey(key)) return;

            INotifyReceivedBrokeredMessages toRemove;
            if (notifiers.TryRemove(key, out toRemove))
            {
                toRemove.Faulted -= NotifierFaulted;
                toRemove.Stop();
                if (eventType != null)
                {
                    logger.InfoFormat("Stopped tracking new notifier for event type {0}, address {1}", eventType.Name, original.ToString());
                }
                else
                {
                    logger.InfoFormat("Stopped tracking new notifier address {1}", original.ToString());
                }
            }
        }

        public INotifyReceivedBrokeredMessages GetNotifier(Type eventType, Address original)
        {
            var key = CreateKeyFor(eventType, original);
            INotifyReceivedBrokeredMessages notifier;
            notifiers.TryGetValue(key, out notifier);
            return notifier;
        }

        void EnqueueMessage(BrokeredMessage brokeredMessage)
        {
            try
            {
                if (brokeredMessage.LockedUntilUtc <= DateTime.UtcNow)
                {
                    logger.Warn("Brokered message lock expired, this could be due to multiple reasons. One of the most common is a mismatch between the lock duration, batch size and processing speed of your handlers. This condition can also happen when there is clock skew between the client and the azure servicebus service.");

                    return;
                }
            }
            catch (InvalidOperationException)
            {
                // if the message was received without a peeklock mechanism you're not allowed to call LockedUntilUtc
                // sadly enough I can't find a public property that checks who the receiver was or if the locktoken has been set
                // those are internal to the sdk
            }

            pendingMessages.Add(brokeredMessage, tokenSource.Token);
        }



        string CreateKeyFor(Type eventType, Address original)
        {
            var key = original.ToString();
            if (eventType != null)
            {
                key += eventType.FullName;
            }
            return key;
        }
        void ErrorDequeueingBatch(Exception ex)
        {
            criticalError.Raise("Fatal messaging exception occured on the broker while dequeueing batch.", ex);
        }
    }
}