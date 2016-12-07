namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Concurrent;
    using Settings;

    class MessageSenderLifeCycleManager : IManageMessageSenderLifeCycle
    {
        ICreateMessageSendersInternal senderFactory;
        int numberOfSendersPerEntity;
        ConcurrentDictionary<string, CircularBuffer<EntityClientEntry>> MessageSenders = new ConcurrentDictionary<string, CircularBuffer<EntityClientEntry>>();

        public MessageSenderLifeCycleManager(ICreateMessageSendersInternal senderFactory, ReadOnlySettings settings)
        {
            this.senderFactory = senderFactory;
            numberOfSendersPerEntity = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity);
        }

        public IMessageSender Get(string entitypath, string viaEntityPath, string namespaceName)
        {
            var buffer = MessageSenders.GetOrAdd(entitypath + viaEntityPath + namespaceName, s =>
            {
                var b = new CircularBuffer<EntityClientEntry>(numberOfSendersPerEntity);
                for(var i = 0; i < numberOfSendersPerEntity; i++)
                {
                    var e = new EntityClientEntry();
                    e.ClientEntity = senderFactory.Create(entitypath, viaEntityPath, namespaceName).GetAwaiter().GetResult();
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
                        entry.ClientEntity = senderFactory.Create(entitypath, viaEntityPath, namespaceName).GetAwaiter().GetResult();
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