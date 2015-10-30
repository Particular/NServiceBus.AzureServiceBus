namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using ObjectBuilder.Common;

    public class TopologyOperator : IOperateTopology
    {
        readonly IContainer container;

        TopologySection topology;

        Func<IncomingMessageDetails, ReceiveContext, Task> onMessage;
        Func<Exception, Task> onError;

        ConcurrentDictionary<EntityInfo, INotifyIncomingMessages> notifiers = new ConcurrentDictionary<EntityInfo, INotifyIncomingMessages>();

        CancellationTokenSource cancellationTokenSource;

        int maxConcurrency;

        public TopologyOperator(IContainer container)
        {
            this.container = container;
        }

        public Task Start(TopologySection topologySection, int maximumConcurrency)
        {
            maxConcurrency = maximumConcurrency;
            topology = topologySection;

            cancellationTokenSource = new CancellationTokenSource();

            return StartNotifiersFor(topology.Entities, maxConcurrency);
        }

        public Task Stop()
        {
            cancellationTokenSource.Cancel();

            return StopNotifiersFor(topology.Entities);
        }

        public Task Start(IEnumerable<EntityInfo> subscriptions)
        {
            return StartNotifiersFor(subscriptions, maxConcurrency);
        }

        public Task Stop(IEnumerable<EntityInfo> subscriptions)
        {
            return StopNotifiersFor(subscriptions);
        }

        public void OnIncomingMessage(Func<IncomingMessageDetails, ReceiveContext, Task> func)
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
                var notifier = notifiers.GetOrAdd(entity, e =>
                {
                    var n = CreateNotifier(entity.Type);
                    n.Initialize(e.Path, e.Namespace.ConnectionString, onMessage, onError, maximumConcurrency);
                    return n;
                });

                if (!notifier.IsRunning)
                {
                    await notifier.Start().ConfigureAwait(false);
                }
                else
                {
                    notifier.RefCount++;
                }
                
            }
        }

        INotifyIncomingMessages CreateNotifier(EntityType type)
        {
            if (type == EntityType.Queue || type == EntityType.Subscription)
            {
                return (INotifyIncomingMessages)container.Build(typeof(MessageReceiverNotifier));
            }

            throw new NotSupportedException("Entity type " + type + " not supported");
        }

        async Task StopNotifiersFor(IEnumerable<EntityInfo> entities)
        {
            foreach (var entity in entities)
            {
                INotifyIncomingMessages notifier;
                notifiers.TryGetValue(entity, out notifier);

                if (notifier == null || !notifier.IsRunning) continue;

                if (notifier.RefCount > 0)
                {
                    notifier.RefCount--;
                }
                else
                {
                    await notifier.Stop().ConfigureAwait(false);
                }
            }
        }
    }
}