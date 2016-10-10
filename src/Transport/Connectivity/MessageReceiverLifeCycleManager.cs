namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Settings;

    class MessageReceiverLifeCycleManager : IManageMessageReceiverLifeCycle
    {
        ICreateMessageReceivers receiveFactory;
        int numberOfReceiversPerEntity;
        ConcurrentDictionary<string, CircularBuffer<EntityClientEntry>> MessageReceivers = new ConcurrentDictionary<string, CircularBuffer<EntityClientEntry>>();

        public MessageReceiverLifeCycleManager(ICreateMessageReceivers receiveFactory, ReadOnlySettings settings)
        {
            this.receiveFactory = receiveFactory;
            numberOfReceiversPerEntity = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity);
        }

        public IMessageReceiver Get(string entityPath, string namespaceAlias)
        {
            var buffer = MessageReceivers.GetOrAdd(entityPath + namespaceAlias, s =>
            {
                var b = new CircularBuffer<EntityClientEntry>(numberOfReceiversPerEntity);
                var exceptions = new ConcurrentQueue<Exception>();
                Parallel.For(0, numberOfReceiversPerEntity, i =>
                {
                    try
                    {
                        var e = new EntityClientEntry();
                        e.ClientEntity = receiveFactory.Create(entityPath, namespaceAlias).GetAwaiter().GetResult();
                        b.Put(e);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Enqueue(ex);
                    }
                   
                });
                if (exceptions.Count > 0) throw new AggregateException(exceptions);
                return b;
            });

            var entry = buffer.Get();

            if (entry.ClientEntity.IsClosed)
            {
                lock (entry.Mutex)
                {
                    if (entry.ClientEntity.IsClosed)
                    {
                        entry.ClientEntity = receiveFactory.Create(entityPath, namespaceAlias).GetAwaiter().GetResult();
                    }
                }
            }

            return entry.ClientEntity;

        }

        class EntityClientEntry
        {
            internal object Mutex = new object();
            internal IMessageReceiver ClientEntity;
        }
    }
}