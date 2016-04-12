namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using NServiceBus.Settings;

    class MessageSenderLifeCycleManager : IManageMessageSenderLifeCycle
    {
        readonly ICreateMessageSenders senderFactory;
        int numberOfSendersPerEntity;
        ConcurrentDictionary<string, CircularBuffer<EntityClientEntry>> MessageSenders = new ConcurrentDictionary<string, CircularBuffer<EntityClientEntry>>();

        public MessageSenderLifeCycleManager(ICreateMessageSenders senderFactory, ReadOnlySettings settings)
        {
            this.senderFactory = senderFactory;
            this.numberOfSendersPerEntity = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity);
        }

        public IMessageSender Get(string entitypath, string viaEntityPath, string namespaceName)
        {
            var buffer = MessageSenders.GetOrAdd(entitypath + viaEntityPath, s =>
            {
                var b = new CircularBuffer<EntityClientEntry>(numberOfSendersPerEntity);
                for (var i = 0; i < numberOfSendersPerEntity; i++)
                {
                    var e = new EntityClientEntry();
                    lock (e.Mutex)
                    {
                        e.ClientEntity = senderFactory.Create(entitypath, viaEntityPath, namespaceName).Result;
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
                        entry.ClientEntity = senderFactory.Create(entitypath, viaEntityPath, namespaceName).Result;
                    }
                }
            }

            return entry.ClientEntity;

        }

        class EntityClientEntry
        {
            internal Object Mutex = new object();
            internal IMessageSender ClientEntity;
        }
    }
}