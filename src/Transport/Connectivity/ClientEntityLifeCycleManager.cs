namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using NServiceBus.Settings;

    class ClientEntityLifeCycleManager
    {
        readonly ICreateEntityClients _factory;
        int _numberOfReceiversPerEntity;
        ConcurrentDictionary<string, CircularBuffer<EntityClientEntry>> MessageReceivers = new ConcurrentDictionary<string, CircularBuffer<EntityClientEntry>>();

        public ClientEntityLifeCycleManager(ICreateEntityClients factory, ReadOnlySettings settings)
        {
            this._factory = factory;
            this._numberOfReceiversPerEntity = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity);
        }

        public IEntityClient Get(string entitypath)
        {
            var buffer = MessageReceivers.GetOrAdd(entitypath, s =>
            {
                var b = new CircularBuffer<EntityClientEntry>(_numberOfReceiversPerEntity);
                for (var i = 0; i < _numberOfReceiversPerEntity; i++)
                {
                    var e = new EntityClientEntry();
                    lock (e.Mutex)
                    {
                        e.Entity = _factory.Create(entitypath);
                    }
                    b.Put(e);
                }
                return b;
            });

            var entry = buffer.Get();

            if (entry.Entity.IsClosed)
            {
                lock (entry.Mutex)
                {
                    if (entry.Entity.IsClosed)
                    {
                        entry.Entity = _factory.Create(entitypath);
                    }
                }
            }

            return entry.Entity;

        }

        class EntityClientEntry
        {
            internal Object Mutex = new object();
            internal IEntityClient Entity;
        }
    }
}