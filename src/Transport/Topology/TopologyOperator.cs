namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using ObjectBuilder.Common;
    using Transports;

    public class TopologyOperator : IOperateTopology
    {
        readonly IContainer container;

        Func<IncomingMessage, ReceiveContext, Task> onMessage;
        Func<Exception, Task> onError;

        ConcurrentDictionary<EntityInfo, INotifyIncomingMessages> notifiers = new ConcurrentDictionary<EntityInfo, INotifyIncomingMessages>();

        CancellationTokenSource cancellationTokenSource;

        int maxConcurrency;

        public TopologyOperator(IContainer container)
        {
            this.container = container;
        }

        public async Task Start(TopologyDefinition topology, int maximumConcurrency)
        {
            this.maxConcurrency = maximumConcurrency;

            cancellationTokenSource = new CancellationTokenSource();

            await StartNotifiersFor(topology.Entities, maxConcurrency);
        }

        public Task Stop()
        {
            cancellationTokenSource.Cancel();

            return Task.FromResult(true);
        }

        public async Task Start(IEnumerable<EntityInfo> subscriptions)
        {
            await StartNotifiersFor(subscriptions, maxConcurrency);
        }

        public Task Stop(IEnumerable<EntityInfo> subscriptions)
        {
            throw new NotImplementedException();
        }

        public void OnIncomingMessage(Func<IncomingMessage, ReceiveContext, Task> func)
        {
            onMessage = func;
        }

        public void OnError(Func<Exception, Task> func)
        {
            onError = func;
        }

        async Task StartNotifiersFor(IEnumerable<EntityInfo> entities, int maximumConcurrency)
        {
            foreach (var entity in entities)
            {
                if (entity.Type == EntityType.Queue || entity.Type == EntityType.Subscription)
                {
                    var notifier = (MessageReceiverNotifier)notifiers.GetOrAdd(entity, e =>
                    {
                        var n = (MessageReceiverNotifier)container.Build(typeof(MessageReceiverNotifier));
                        n.Initialize(e.Path, e.Namespace.ConnectionString, onMessage, onError, maximumConcurrency);
                        return null;
                    });

                    await notifier.Start();
                }
            }
        }
    }
}