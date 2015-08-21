namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using NServiceBus.Settings;

    class MessagingFactoryLifeCycleManager
    {
        int _numberOfFactoriesPerNamespace;
        ICreateMessagingFactories _createMessagingFactories;
        ConcurrentDictionary<string, CircularBuffer<FactoryEntry>> MessagingFactories = new ConcurrentDictionary<string, CircularBuffer<FactoryEntry>>();

        public MessagingFactoryLifeCycleManager(ICreateMessagingFactories createMessagingFactories, ReadOnlySettings settings)
        {
            this._createMessagingFactories = createMessagingFactories;
            this._numberOfFactoriesPerNamespace = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfMessagingFactoriesPerNamespace);
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

        class FactoryEntry
        {
            internal Object Mutex = new object();
            internal IMessagingFactory Factory;
        }
    }
}