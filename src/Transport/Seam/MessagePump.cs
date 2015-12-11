namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Extensibility;
    using Transports;

    class MessagePump : IPushMessages, IDisposable
    {
        ITopologySectionManager topologySectionManager;
        IOperateTopology topologyOperator;
        Func<PushContext, Task> messagePump;
        private RepeatedFailuresOverTimeCircuitBreaker circuitBreaker;

        public MessagePump(ITopologySectionManager topologySectionManager, IOperateTopology topologyOperator)
        {
            this.topologySectionManager = topologySectionManager;
            this.topologyOperator = topologyOperator;
        }

        public void Init(Func<PushContext, Task> pump, PushSettings settings)
        {
            messagePump = pump;

            //TODO: integrate these
            //settings.ErrorQueue
            //settings.InputQueue
            //settings.PurgeOnStartup
            //settings.RequiredConsistency

            

            topologyOperator.OnIncomingMessage((incoming, receiveContext) =>
                {
                    circuitBreaker?.Success();

                    var context = new ContextBag();

                    context.Set(receiveContext);

                    //todo, figure out what the TransportTransaction parameter is about
                    return messagePump(new PushContext(incoming.MessageId, incoming.Headers, incoming.BodyStream, new NoTransaction(), context));
                });

        }

        public void OnCriticalError(CriticalError criticalError)
        {
            circuitBreaker = new RepeatedFailuresOverTimeCircuitBreaker("MessagePump", TimeSpan.FromSeconds(30), ex => criticalError.Raise("Failed to receive message from Azure Service Bus.", ex));
        }

        public void OnError(Func<Exception, Task> func)
        {
            topologyOperator.OnError(exception =>
            {
                circuitBreaker?.Failure(exception);
                return func(exception);
            });
        }

        public void Start(PushRuntimeSettings limitations)
        {
            var definition = topologySectionManager.DetermineReceiveResources();
            topologyOperator.Start(definition, limitations.MaxConcurrency);
        }

        public Task Stop()
        {
            return topologyOperator.Stop();
        }

        public void Dispose()
        {
            // Injected
        }
    }

    public class NoTransaction : TransportTransaction { }
}