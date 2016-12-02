namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensibility;
    using Logging;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Topology.MetaModel;
    using Settings;

    class MessagePump : IPushMessages, IDisposable
    {
        public MessagePump(ITopologySectionManager topologySectionManager, ITransportPartsContainer container, ReadOnlySettings settings) : this(topologySectionManager, container, settings, TimeSpan.FromSeconds(30))
        {
        }

        internal MessagePump(ITopologySectionManager topologySectionManager, ITransportPartsContainer container, ReadOnlySettings settings, TimeSpan timeToWaitBeforeTriggeringTheCircuitBreaker)
        {
            this.topologySectionManager = topologySectionManager;
            this.container = container;
            satelliteTransportAddresses = settings.Get<SatelliteTransportAddressCollection>();
            timeToWaitBeforeTriggering = timeToWaitBeforeTriggeringTheCircuitBreaker;
        }

        public void Dispose()
        {
            // Injected
        }

        public Task Init(Func<MessageContext, Task> pump, Func<ErrorContext, Task<ErrorHandleResult>> onError, CriticalError criticalError, PushSettings pushSettings)
        {
            topologyOperator = DetermineTopologyOperator(pushSettings.InputQueue);

            messagePump = pump;
            var name = $"MessagePump on the queue `{pushSettings.InputQueue}`";
            circuitBreaker = new RepeatedFailuresOverTimeCircuitBreaker(name, timeToWaitBeforeTriggering, ex => criticalError.Raise("Failed to receive message from Azure Service Bus.", ex));

            if (pushSettings.PurgeOnStartup)
            {
                throw new InvalidOperationException("Azure Service Bus transport doesn't support PurgeOnStartup behavior");
            }

            inputQueue = pushSettings.InputQueue;


            topologyOperator.OnIncomingMessage(async (incoming, receiveContext) =>
            {
                var tokenSource = new CancellationTokenSource();
                receiveContext.CancellationToken = tokenSource.Token;

                circuitBreaker.Success();

                var transportTransaction = new TransportTransaction();
                transportTransaction.Set(receiveContext);

                await throttler.WaitAsync(receiveContext.CancellationToken).ConfigureAwait(false);

                try
                {
                    await messagePump(new MessageContext(incoming.MessageId, incoming.Headers, incoming.Body, transportTransaction, tokenSource, new ContextBag())).ConfigureAwait(false);
                }
                finally
                {
                    throttler.Release();
                }
            });

            topologyOperator.OnError(exception => circuitBreaker.Failure(exception));
            topologyOperator.OnProcessingFailure(onError);

            return TaskEx.Completed;
        }

        public void Start(PushRuntimeSettings limitations)
        {
            var definition = topologySectionManager.DetermineReceiveResources(inputQueue);
            throttler = new SemaphoreSlim(limitations.MaxConcurrency);
            topologyOperator.Start(definition, limitations.MaxConcurrency);
        }

        public async Task Stop()
        {
            logger.Info($"Stopping '{inputQueue}' messagepump");

            await topologyOperator.Stop().ConfigureAwait(false);

            logger.Info($"Messagepump '{inputQueue}' stopped");
        }

        /// <summary>
        /// Determine what topology operator to use.
        /// For the main input queue, cache and re-use the same topology operator.
        /// For satellite input queues, create a new topology operator.
        /// </summary>
        IOperateTopology DetermineTopologyOperator(string pushSettingsInputQueue)
        {
            if (satelliteTransportAddresses.Contains(pushSettingsInputQueue))
            {
                return new TopologyOperator(container);
            }

            return container.Resolve<IOperateTopology>();
        }

        readonly ITransportPartsContainer container;
        ITopologySectionManager topologySectionManager;
        IOperateTopology topologyOperator;
        Func<MessageContext, Task> messagePump;
        RepeatedFailuresOverTimeCircuitBreaker circuitBreaker;
        ILog logger = LogManager.GetLogger(typeof(MessagePump));
        string inputQueue;
        SatelliteTransportAddressCollection satelliteTransportAddresses;
        SemaphoreSlim throttler;
        TimeSpan timeToWaitBeforeTriggering;
    }
}