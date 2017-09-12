namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Settings;

    class MessageReceiverLifeCycleManager
    {
        public MessageReceiverLifeCycleManager(ICreateMessageReceiversInternal receiveFactory, ReadOnlySettings settings)
        {
            this.receiveFactory = receiveFactory;
            numberOfReceiversPerEntity = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity);
        }

        public async Task<IMessageReceiverInternal> Get(string entityPath, string namespaceAlias)
        {
            var buffer = await MessageReceivers.GetOrAdd(entityPath + namespaceAlias, async s =>
            {
                var b = new CircularBuffer<EntityClientEntry>(numberOfReceiversPerEntity);
                for (var i = 0; i < numberOfReceiversPerEntity; i++)
                {
                    var e = new EntityClientEntry
                    {
                        ClientEntity = await receiveFactory.Create(entityPath, namespaceAlias)
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
                    entry.ClientEntity = await receiveFactory.Create(entityPath, namespaceAlias)
                        .ConfigureAwait(false);
                }
                return entry.ClientEntity;
            }
            finally
            {
                entry.Semaphore.Release();
            }
        }

        ICreateMessageReceiversInternal receiveFactory;
        int numberOfReceiversPerEntity;
        ConcurrentDictionary<string, Task<CircularBuffer<EntityClientEntry>>> MessageReceivers = new ConcurrentDictionary<string, Task<CircularBuffer<EntityClientEntry>>>();

        class EntityClientEntry
        {
            internal SemaphoreSlim Semaphore = new SemaphoreSlim(1);
            internal IMessageReceiverInternal ClientEntity;
        }
    }
}