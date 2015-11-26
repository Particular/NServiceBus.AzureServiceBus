namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Extensibility;
    using Transports;

    class MessagePump : IPushMessages
    {
        ITopologySectionManager topologySectionManager;
        IOperateTopology topologyOperator;
        Func<PushContext, Task> messagePump;

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
                var context = new ContextBag();

                context.Set(receiveContext);

                //todo, figure out what the TransportTransaction parameter is about
                return messagePump(new PushContext(incoming.MessageId, incoming.Headers, incoming.BodyStream, new NoTransaction(), context));
            });

        }

        public void OnError(Func<Exception, Task> func)
        {
            topologyOperator.OnError(func);
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
    }

    public class NoTransaction : TransportTransaction { }
}