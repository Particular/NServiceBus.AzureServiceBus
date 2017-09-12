namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Settings;

    class MessageSenderLifeCycleManager
    {
        public MessageSenderLifeCycleManager(ICreateMessageSendersInternal senderFactory, ReadOnlySettings settings)
        {
            this.senderFactory = senderFactory;
            numberOfSendersPerEntity = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity);
        }

        public async Task<IMessageSenderInternal> Get(string entitypath, string viaEntityPath, string namespaceName)
        {
            var buffer = await MessageSenders.GetOrAdd(entitypath + viaEntityPath + namespaceName, async s =>
            {
                var b = new CircularBuffer<EntityClientEntry>(numberOfSendersPerEntity);
                for (var i = 0; i < numberOfSendersPerEntity; i++)
                {
                    var e = new EntityClientEntry
                    {
                        ClientEntity = await senderFactory.Create(entitypath, viaEntityPath, namespaceName)
                            .ConfigureAwait(false)
                    };
                    b.Put(e);
                }

                return b;
            }).ConfigureAwait(false);

            var entry = buffer.Get();

            if (!entry.ClientEntity.IsClosed)
            {
                return entry.ClientEntity;
            }

            try
            {
                await entry.Semaphore.WaitAsync()
                    .ConfigureAwait(false);
                
                if (entry.ClientEntity.IsClosed)
                {
                    entry.ClientEntity = await senderFactory.Create(entitypath, viaEntityPath, namespaceName)
                        .ConfigureAwait(false);
                }

                return entry.ClientEntity;
            }
            finally
            {
                entry.Semaphore.Release();
            }
        }

        ICreateMessageSendersInternal senderFactory;
        int numberOfSendersPerEntity;
        ConcurrentDictionary<string, Task<CircularBuffer<EntityClientEntry>>> MessageSenders = new ConcurrentDictionary<string, Task<CircularBuffer<EntityClientEntry>>>();

        class EntityClientEntry
        {
            internal SemaphoreSlim Semaphore = new SemaphoreSlim(1);
            internal IMessageSenderInternal ClientEntity;
        }
    }
}