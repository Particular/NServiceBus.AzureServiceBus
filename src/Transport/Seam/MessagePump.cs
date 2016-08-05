namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensibility;
    using Logging;
    using NServiceBus.AzureServiceBus;
    using Transport;

    class MessagePump : IPushMessages, IDisposable
    {
        ITopologySectionManager topologySectionManager;
        IOperateTopology topologyOperator;
        Func<MessageContext, Task> messagePump;
        RepeatedFailuresOverTimeCircuitBreaker circuitBreaker;
        ILog logger = LogManager.GetLogger(typeof(MessagePump));
        string inputQueue;

        public MessagePump(ITopologySectionManager topologySectionManager, IOperateTopology topologyOperator)
        {
            this.topologySectionManager = topologySectionManager;
            this.topologyOperator = topologyOperator;
        }

        public Task Init(Func<MessageContext, Task> pump, Func<ErrorContext, Task<ErrorHandleResult>> onError, CriticalError criticalError, PushSettings pushSettings)
        {
            messagePump = pump;
            var name = $"MessagePump on the queue `{pushSettings.InputQueue}`";
            circuitBreaker = new RepeatedFailuresOverTimeCircuitBreaker(name, TimeSpan.FromSeconds(30), ex => criticalError.Raise("Failed to receive message from Azure Service Bus.", ex));

            if (pushSettings.PurgeOnStartup)
            {
                throw new InvalidOperationException("Azure Service Bus transport doesn't support PurgeOnStartup behavior");
            }

            inputQueue = pushSettings.InputQueue;

            topologyOperator.OnIncomingMessage( (incoming, receiveContext) =>
            {
                var tokenSource = new CancellationTokenSource();
                receiveContext.CancellationToken = tokenSource.Token;

                circuitBreaker.Success();
               
                var transportTransaction = new TransportTransaction();
                transportTransaction.Set(receiveContext);

                return messagePump(new MessageContext(incoming.MessageId, incoming.Headers, incoming.Body, transportTransaction, tokenSource, new ContextBag()));
                
            });

            topologyOperator.OnProcessingFailure(onError);

            return TaskEx.Completed;
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