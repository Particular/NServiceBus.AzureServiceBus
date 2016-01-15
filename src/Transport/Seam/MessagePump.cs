namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensibility;
    using NServiceBus.Logging;
    using Transports;

    class MessagePump : IPushMessages, IDisposable
    {
        ITopologySectionManager topologySectionManager;
        IOperateTopology topologyOperator;
        Func<PushContext, Task> messagePump;
        private RepeatedFailuresOverTimeCircuitBreaker circuitBreaker;
        ILog logger = LogManager.GetLogger(typeof(MessagePump));
        string _inputQueue ;

        public MessagePump(ITopologySectionManager topologySectionManager, IOperateTopology topologyOperator)
        {
            this.topologySectionManager = topologySectionManager;
            this.topologyOperator = topologyOperator;
        }

        public Task Init(Func<PushContext, Task> pump, CriticalError criticalError, PushSettings settings)
        {
            messagePump = pump;
            circuitBreaker = new RepeatedFailuresOverTimeCircuitBreaker("MessagePump", TimeSpan.FromSeconds(30), ex => criticalError.Raise("Failed to receive message from Azure Service Bus.", ex));

            //TODO: integrate these
            //settings.ErrorQueue
            _inputQueue = settings.InputQueue;
            //settings.PurgeOnStartup
            //settings.RequiredConsistency

            var tokensource = new CancellationTokenSource();

            topologyOperator.OnIncomingMessage((incoming, receiveContext) =>
                {
                    circuitBreaker?.Success();

                    var context = new ContextBag();

                    context.Set(receiveContext);

                    //todo, figure out what the TransportTransaction parameter is about
                    return messagePump(new PushContext(incoming.MessageId, incoming.Headers, incoming.BodyStream, new NoTransaction(), tokensource, context));
                });

            return TaskEx.Completed;
        }

        /// <summary>For internal testing purposes.</summary>
        internal void OnError(Func<Exception, Task> func)
        {
            topologyOperator.OnError(exception =>
            {
                circuitBreaker?.Failure(exception);
                return func(exception);
            });
        }

        public void Start(PushRuntimeSettings limitations)
        {
            var definition = topologySectionManager.DetermineReceiveResources(_inputQueue);
            topologyOperator.Start(definition, limitations.MaxConcurrency);
        }

        public async Task Stop()
        {
            logger.Info("Stopping messagepump");

            await topologyOperator.Stop();

            logger.Info("Messagepump stopped");
        }

        public void Dispose()
        {
            // Injected
        }
    }

    public class NoTransaction : TransportTransaction { }
}