namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using NServiceBus.Settings;

    class MessagingFactoryLifeCycleManager : IManageMessagingFactoryLifeCycle
    {
        int _numberOfFactoriesPerNamespace;
        ICreateMessagingFactories _createMessagingFactories;
        ConcurrentDictionary<string, CircularBuffer<FactoryEntry>> MessagingFactories = new ConcurrentDictionary<string, CircularBuffer<FactoryEntry>>();

        public MessagingFactoryLifeCycleManager(ICreateMessagingFactories createMessagingFactories, ReadOnlySettings settings)
        {
            this._createMessagingFactories = createMessagingFactories;
            this._numberOfFactoriesPerNamespace = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace);
        }

        public IMessagingFactory Get(string @namespace)
        {
            var buffer = MessagingFactories.GetOrAdd(@namespace, s =>
            {
                var b = new CircularBuffer<FactoryEntry>(_numberOfFactoriesPerNamespace);
                for (var i = 0; i < _numberOfFactoriesPerNamespace; i++)
                {
                    var e = new FactoryEntry();
                    lock (e.Mutex)
                    {
                        e.Factory = _createMessagingFactories.Create(@namespace);
                    }
                    b.Put(e);
                }
                return b;
            });

            var entry = buffer.Get();

            if (entry.Factory.IsClosed)
            {
                lock (entry.Mutex)
                {
                    if (entry.Factory.IsClosed)
                    {
                        entry.Factory = _createMessagingFactories.Create(@namespace);
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
            internal Object Mutex = new object();
            internal IMessagingFactory Factory;
        }
    }
}