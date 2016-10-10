namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Settings;

    class MessageSenderLifeCycleManager : IManageMessageSenderLifeCycle
    {
        ICreateMessageSenders senderFactory;
        int numberOfSendersPerEntity;
        ConcurrentDictionary<string, CircularBuffer<EntityClientEntry>> MessageSenders = new ConcurrentDictionary<string, CircularBuffer<EntityClientEntry>>();

        public MessageSenderLifeCycleManager(ICreateMessageSenders senderFactory, ReadOnlySettings settings)
        {
            this.senderFactory = senderFactory;
            numberOfSendersPerEntity = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity);
        }

        public IMessageSender Get(string entitypath, string viaEntityPath, string namespaceName)
        {
            var buffer = MessageSenders.GetOrAdd(entitypath + viaEntityPath + namespaceName, s =>
            {
                var b = new CircularBuffer<EntityClientEntry>(numberOfSendersPerEntity);
                var exceptions = new ConcurrentQueue<Exception>();
                Parallel.For(0, numberOfSendersPerEntity, i =>
                {
                    try
                    {
                        var e = new EntityClientEntry();
                        e.ClientEntity = senderFactory.Create(entitypath, viaEntityPath, namespaceName).GetAwaiter().GetResult();
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
                        entry.ClientEntity = senderFactory.Create(entitypath, viaEntityPath, namespaceName).Result;
                    }
                }
            }

            return entry.ClientEntity;

        }

        class EntityClientEntry
        {
            internal object Mutex = new object();
            internal IMessageSender ClientEntity;
        }
    }
}