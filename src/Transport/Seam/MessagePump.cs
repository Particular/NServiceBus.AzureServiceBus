namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensibility;
    using Logging;
    using Transports;

    class MessagePump : IPushMessages, IDisposable
    {
        ITopologySectionManager topologySectionManager;
        IOperateTopology topologyOperator;
        Func<PushContext, Task> messagePump;
        RepeatedFailuresOverTimeCircuitBreaker circuitBreaker;
        ILog logger = LogManager.GetLogger(typeof(MessagePump));
        string inputQueue;

        public MessagePump(ITopologySectionManager topologySectionManager, IOperateTopology topologyOperator)
        {
            this.topologySectionManager = topologySectionManager;
            this.topologyOperator = topologyOperator;
        }

        public Task Init(Func<PushContext, Task> pump, CriticalError criticalError, PushSettings pushSettings)
        {
            messagePump = pump;
            var name = $"MessagePump on the queue `{pushSettings.InputQueue}`";
            circuitBreaker = new RepeatedFailuresOverTimeCircuitBreaker(name, TimeSpan.FromSeconds(30), ex => criticalError.Raise("Failed to receive message from Azure Service Bus.", ex));

            if (pushSettings.PurgeOnStartup)
            {
                throw new InvalidOperationException("Azure Service Bus transport doesn't support PurgeOnStartup behaviour");
            }

            inputQueue = pushSettings.InputQueue;

            topologyOperator.OnIncomingMessage((incoming, receiveContext) =>
            {
                var tokenSource = new CancellationTokenSource();
                receiveContext.CancellationToken = tokenSource.Token;

                circuitBreaker.Success();

                var context = new ContextBag();

                context.Set(receiveContext);

                //todo, figure out what the TransportTransaction parameter is about
                return messagePump(new PushContext(incoming.MessageId, incoming.Headers, incoming.BodyStream, new NoTransaction(), tokenSource, context));
            });

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

    class NoTransaction : TransportTransaction { }
}