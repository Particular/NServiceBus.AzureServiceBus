namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using NServiceBus.Settings;

    class ClientEntityLifeCycleManager
    {
        readonly ICreateClientEntities _factory;
        int _numberOfReceiversPerEntity;
        ConcurrentDictionary<string, CircularBuffer<EntityClientEntry>> MessageReceivers = new ConcurrentDictionary<string, CircularBuffer<EntityClientEntry>>();

        public ClientEntityLifeCycleManager(ICreateClientEntities factory, ReadOnlySettings settings)
        {
            this._factory = factory;
            this._numberOfReceiversPerEntity = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity);
        }

        public IClientEntity Get(string entitypath, string connectionstring)
        {
            var buffer = MessageReceivers.GetOrAdd(entitypath, s => 
            {
                var b = new CircularBuffer<EntityClientEntry>(_numberOfReceiversPerEntity);
                for (var i = 0; i < _numberOfReceiversPerEntity; i++)
                {
                    var e = new EntityClientEntry();
                    lock (e.Mutex)
                    {
                        e.ClientEntity = _factory.CreateAsync(entitypath, connectionstring).Result;
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
                        entry.ClientEntity = _factory.CreateAsync(entitypath, connectionstring).Result;
                    }
                }
            }

            return entry.ClientEntity;

        }

        class EntityClientEntry
        {
            internal Object Mutex = new object();
            internal IClientEntity ClientEntity;
        }
    }
}