namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Extensibility;
    using Transports;

    class MessagePump : IPushMessages
    {
        ITopology topology;
        IOperateTopology topologyOperator;
        Func<PushContext, Task> messagePump;

        public MessagePump(ITopology topology, IOperateTopology topologyOperator)
        {
            this.topology = topology;
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

                return messagePump(new PushContext(incoming.MessageId, incoming.Headers, incoming.BodyStream, context));
            });

        }

        public void OnError(Func<Exception, Task> func)
        {
            topologyOperator.OnError(func);
        }

        public void Start(PushRuntimeSettings limitations)
        {
            var definition = topology.DetermineReceiveResources();
            topologyOperator.Start(definition, limitations.MaxConcurrency);
        }

        public Task Stop()
        {
            return topologyOperator.StopAsync();
        }
    }
}