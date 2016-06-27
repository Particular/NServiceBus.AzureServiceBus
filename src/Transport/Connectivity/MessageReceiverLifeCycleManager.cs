namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
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

        public IMessageReceiver Get(string entitypath, string namespaceName)
        {
            var buffer = MessageReceivers.GetOrAdd(entitypath, s =>
            {
                var b = new CircularBuffer<EntityClientEntry>(numberOfReceiversPerEntity);
                for (var i = 0; i < numberOfReceiversPerEntity; i++)
                {
                    var e = new EntityClientEntry();
                    lock (e.Mutex)
                    {
                        e.ClientEntity = receiveFactory.Create(entitypath, namespaceName).Result;
                    }
                    b.Put(e);
                }
                return b;
            });

            var entry = buffer.Get();

            if (entry.ClientEntity.IsClosed)
            {
                lock (entry.Mutex)
                {
                    if (entry.ClientEntity.IsClosed)
                    {
                        entry.ClientEntity = receiveFactory.Create(entitypath, namespaceName).Result;
                    }
                }
            }

            return entry.ClientEntity;

        }

        class EntityClientEntry
        {
            internal Object Mutex = new object();
            internal IMessageReceiver ClientEntity;
        }
    }
}