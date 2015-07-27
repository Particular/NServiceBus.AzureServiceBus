namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Settings;

    class MessageReceiverLifeCycleManager
    {
        readonly ICreateMessageReceivers _factory;
        int _numberOfReceiversPerEntity;
        ConcurrentDictionary<string, CircularBuffer<ReceiverEntry>> MessageReceivers = new ConcurrentDictionary<string, CircularBuffer<ReceiverEntry>>();

        public MessageReceiverLifeCycleManager(ICreateMessageReceivers factory, ReadOnlySettings settings)
        {
            this._factory = factory;
            this._numberOfReceiversPerEntity = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfMessageReceiversPerEntity);
        }

        public MessageReceiver Get(string entitypath)
        {
            var buffer = MessageReceivers.GetOrAdd(entitypath, s =>
            {
                var b = new CircularBuffer<ReceiverEntry>(_numberOfReceiversPerEntity);
                for (var i = 0; i < _numberOfReceiversPerEntity; i++)
                {
                    var e = new ReceiverEntry();
                    lock (e.Mutex)
                    {
                        e.Receiver = _factory.Create(entitypath);
                    }
                    b.Put(e);
                }
                return b;
            });

            var entry = buffer.Get();

            if (entry.Receiver.IsClosed)
            {
                lock (entry.Mutex)
                {
                    if (entry.Receiver.IsClosed)
                    {
                        entry.Receiver = _factory.Create(entitypath);
                    }
                }
            }

            return entry.Receiver;

        }

        class ReceiverEntry
        {
            internal Object Mutex = new object();
            internal MessageReceiver Receiver;
        }
    }
}