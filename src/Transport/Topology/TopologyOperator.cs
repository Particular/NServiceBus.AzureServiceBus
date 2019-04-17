namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Logging;

    class TopologyOperator : IOperateTopology, IDisposable
    {
        public TopologyOperator(ITransportPartsContainer container)
        {
            this.container = container;
        }

        public void Dispose()
        {
            // Injected
        }

        public void Start(TopologySection topologySection, int maximumConcurrency)
        {
            maxConcurrency = maximumConcurrency;
            topology = topologySection;

            StartNotifiersFor(topology.Entities);

            running = true;

            Action operation;
            while (pendingStartOperations.TryTake(out operation))
            {
                operation();
            }
        }

        public Task Stop()
        {
            logger.Info("Stopping notifiers");
            return StopNotifiersForAsync(topology.Entities);
        }

        public void Start(IEnumerable<EntityInfo> subscriptions)
        {
            if (!running) // cannot start subscribers before the notifier itself is started
            {
                pendingStartOperations.Add(() => StartNotifiersFor(subscriptions));
            }
            else
            {
                StartNotifiersFor(subscriptions);
            }
        }

        public Task Stop(IEnumerable<EntityInfo> subscriptions)
        {
            return StopNotifiersForAsync(subscriptions);
        }

        public void OnIncomingMessage(Func<IncomingMessageDetails, ReceiveContext, Task> func)
        {
            onMessage = func;
        }

        public void OnError(Func<Exception, Task> func)
        {
            onError = func;
        }

        public void OnProcessingFailure(Func<ErrorContext, Task<ErrorHandleResult>> func)
        {
            onProcessingFailure = func;
        }

        void StartNotifiersFor(IEnumerable<EntityInfo> entities)
        {
            var criticalError = container.Resolve<CriticalError>();

            foreach (var entity in entities)
            {
                if (!entity.ShouldBeListenedTo)
                    continue;

                var notifier = notifiers.GetOrAdd(entity, e =>
                {
                    var n = CreateNotifier(entity.Type);
                    n.Initialize(e, onMessage, onError, onProcessingFailure, maxConcurrency);
                    n.CriticalError = criticalError;
                    return n;
                });

                notifier.Start();
            }
        }

        INotifyIncomingMessagesWithCriticalError CreateNotifier(EntityType type)
        {
            if (type == EntityType.Queue || type == EntityType.Subscription)
            {
                return (INotifyIncomingMessagesWithCriticalError) container.Resolve(typeof(MessageReceiverNotifier));
            }

            throw new NotSupportedException("Entity type " + type + " not supported");
        }

        async Task StopNotifiersForAsync(IEnumerable<EntityInfo> entities)
        {
            foreach (var entity in entities)
            {
                INotifyIncomingMessages notifier;
                notifiers.TryGetValue(entity, out notifier);

                if (notifier == null)
                {
                    continue;
                }

                await notifier.Stop().ConfigureAwait(false);
            }
        }

        ITransportPartsContainer container;

        TopologySection topology;

        Func<IncomingMessageDetails, ReceiveContext, Task> onMessage;
        Func<Exception, Task> onError;
        Func<ErrorContext, Task<ErrorHandleResult>> onProcessingFailure;

        ConcurrentDictionary<EntityInfo, INotifyIncomingMessages> notifiers = new ConcurrentDictionary<EntityInfo, INotifyIncomingMessages>();

        volatile bool running;
        ConcurrentBag<Action> pendingStartOperations = new ConcurrentBag<Action>();
        ILog logger = LogManager.GetLogger(typeof(TopologyOperator));

        int maxConcurrency;
    }
}