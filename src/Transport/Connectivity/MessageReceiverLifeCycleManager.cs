namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using NServiceBus.Settings;

    class MessageReceiverLifeCycleManager : IManageMessageReceiverLifeCycle
    {
        readonly ICreateMessageReceivers _receiveFactory;
        int _numberOfReceiversPerEntity;
        ConcurrentDictionary<string, CircularBuffer<EntityClientEntry>> MessageReceivers = new ConcurrentDictionary<string, CircularBuffer<EntityClientEntry>>();

        public MessageReceiverLifeCycleManager(ICreateMessageReceivers receiveFactory, ReadOnlySettings settings)
        {
            this._receiveFactory = receiveFactory;
            this._numberOfReceiversPerEntity = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity);
        }

        public IMessageReceiver Get(string entitypath, string connectionstring)
        {
            var buffer = MessageReceivers.GetOrAdd(entitypath, s => 
            {
                var b = new CircularBuffer<EntityClientEntry>(_numberOfReceiversPerEntity);
                for (var i = 0; i < _numberOfReceiversPerEntity; i++)
                {
                    var e = new EntityClientEntry();
                    lock (e.Mutex)
                    {
                        e.ClientEntity = _receiveFactory.CreateAsync(entitypath, connectionstring).Result;
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
                        entry.ClientEntity = _receiveFactory.CreateAsync(entitypath, connectionstring).Result;
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