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
    using Transport;

    class MessagePump : IPushMessages, IDisposable
    {
        ITopologySectionManager topologySectionManager;
        readonly ITransportPartsContainer container;
        IOperateTopology topologyOperator;
        Func<MessageContext, Task> messagePump;
        RepeatedFailuresOverTimeCircuitBreaker circuitBreaker;
        ILog logger = LogManager.GetLogger(typeof(MessagePump));
        string inputQueue;
        SatelliteTransportAddressCollection satelliteTransportAddresses;
        SemaphoreSlim throttler = new SemaphoreSlim(1);

        public MessagePump(ITopologySectionManager topologySectionManager, ITransportPartsContainer container, ReadOnlySettings settings)
        {
            this.topologySectionManager = topologySectionManager;
            this.container = container;
            satelliteTransportAddresses = settings.Get<SatelliteTransportAddressCollection>();
        }

        public Task Init(Func<MessageContext, Task> pump, Func<ErrorContext, Task<ErrorHandleResult>> onError, CriticalError criticalError, PushSettings pushSettings)
        {
            topologyOperator = DetermineTopologyOperator(pushSettings.InputQueue);

            messagePump = pump;
            var name = $"MessagePump on the queue `{pushSettings.InputQueue}`";
            circuitBreaker = new RepeatedFailuresOverTimeCircuitBreaker(name, TimeSpan.FromSeconds(30), ex => criticalError.Raise("Failed to receive message from Azure Service Bus.", ex));

            if (pushSettings.PurgeOnStartup)
            {
                throw new InvalidOperationException("Azure Service Bus transport doesn't support PurgeOnStartup behavior");
            }

            inputQueue = pushSettings.InputQueue;

            
            topologyOperator.OnIncomingMessage((incoming, receiveContext) =>
            {
               var tokenSource = new CancellationTokenSource();
               receiveContext.CancellationToken = tokenSource.Token;

               circuitBreaker.Success();

               var transportTransaction = new TransportTransaction();
               transportTransaction.Set(receiveContext);

                return Task.Run(async () =>
                {
                    await throttler.WaitAsync(receiveContext.CancellationToken).ConfigureAwait(false);

                    try
                    {
                       await messagePump(new MessageContext(incoming.MessageId, incoming.Headers, incoming.Body, transportTransaction, tokenSource, new ContextBag())).ConfigureAwait(false);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }, receiveContext.CancellationToken);
              
            });

            topologyOperator.OnProcessingFailure(onError);

            return TaskEx.Completed;
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

        // For internal testing purposes.
        internal void OnError(Func<Exception, Task> func)
        {
            topologyOperator.OnError(async exception =>
            {
                await circuitBreaker.Failure(exception).ConfigureAwait(false);
                await func(exception).ConfigureAwait(false);
            });
        }

        public void Start(PushRuntimeSettings limitations)
        {
            var definition = topologySectionManager.DetermineReceiveResources(inputQueue);
            throttler = new SemaphoreSlim(limitations.MaxConcurrency);
            topologyOperator.Start(definition, limitations.MaxConcurrency);
        }

        public async Task Stop()
        {
            logger.Info("Stopping messagepump");

            await topologyOperator.Stop().ConfigureAwait(false);

            logger.Info("Messagepump stopped");
        }

        public void Dispose()
        {
            // Injected
        }
    }
}