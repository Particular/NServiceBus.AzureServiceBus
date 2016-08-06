namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Settings;

    class MessagingFactoryLifeCycleManager : IManageMessagingFactoryLifeCycle
    {
        int numberOfFactoriesPerNamespace;
        ICreateMessagingFactories createMessagingFactories;
        ConcurrentDictionary<string, CircularBuffer<FactoryEntry>> MessagingFactories = new ConcurrentDictionary<string, CircularBuffer<FactoryEntry>>();

        public MessagingFactoryLifeCycleManager(ICreateMessagingFactories createMessagingFactories, ReadOnlySettings settings)
        {
            this.createMessagingFactories = createMessagingFactories;
            numberOfFactoriesPerNamespace = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace);
        }

        public IMessagingFactory Get(string namespaceName)
        {
            var buffer = MessagingFactories.GetOrAdd(namespaceName, s =>
            {
                var b = new CircularBuffer<FactoryEntry>(numberOfFactoriesPerNamespace);

                var factories = new List<FactoryEntry>(numberOfFactoriesPerNamespace);
                for (var i = 0; i < numberOfFactoriesPerNamespace; i++)
                {
                    var factory = createMessagingFactories.Create(namespaceName);
                    factories.Add(new FactoryEntry { Factory = factory });
                }

                b.Put(factories.ToArray());
                return b;
            });

            var entry = buffer.Get();

            if (entry.Factory.IsClosed)
            {
                lock (entry.Mutex)
                {
                    if (entry.Factory.IsClosed)
                    {
                        entry.Factory = createMessagingFactories.Create(namespaceName);
                    }
                }
            }

            return entry.Factory;

        }

        public async Task CloseAll()
        {
            foreach (var key in MessagingFactories.Keys)
            {
                var buffer = MessagingFactories[key];
                foreach (var entry in buffer.GetBuffer())
                {
                    await entry.Factory.CloseAsync().ConfigureAwait(false);
                }
            }
        }

        class FactoryEntry
        {
            internal object Mutex = new object();
            internal IMessagingFactory Factory;
        }
    }
}