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
        Func<PushContext, Task> pipeline;

        public MessagePump(ITopology topology, IOperateTopology topologyOperator)
        {
            this.topology = topology;
            this.topologyOperator = topologyOperator;
        }

        public void Init(Func<PushContext, Task> pipe, PushSettings settings)
        {
            pipeline = pipe;

            //TODO: integrate these
            //settings.ErrorQueue
            //settings.InputQueue
            //settings.PurgeOnStartup
            //settings.RequiredConsistency

            topologyOperator.OnIncomingMessage((incoming, receiveContext) =>
            {
                var context = new ContextBag();

                context.Set(receiveContext);

                return pipeline(new PushContext(incoming, context));
            });

        }

        public void OnError(Func<Exception, Task> func)
        {
            topologyOperator.OnError(func);
        }

        public async void Start(PushRuntimeSettings limitations)
        {
            var definition = topology.Determine(Purpose.Receiving);
            await topologyOperator.Start(definition, limitations.MaxConcurrency).ConfigureAwait(false);
        }

        public async Task Stop()
        {
            await topologyOperator.Stop().ConfigureAwait(false);
        }
    }
}