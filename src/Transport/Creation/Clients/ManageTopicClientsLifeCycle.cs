namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using Microsoft.ServiceBus.Messaging;

    class ManageTopicClientsLifeCycle : IManageTopicClientsLifecycle
    {
        const int numberOfTopicClientsPerAddress = 4;

        ICreateTopicClients topicClientCreator;
        readonly IManageMessagingFactoriesLifecycle messagingFactories;

        ConcurrentDictionary<string, CircularBuffer<TopicClientEntry>> topicClients = new ConcurrentDictionary<string, CircularBuffer<TopicClientEntry>>();

        public ManageTopicClientsLifeCycle(ICreateTopicClients topicClientCreator, IManageMessagingFactoriesLifecycle messagingFactories)
        {
            this.topicClientCreator = topicClientCreator;
            this.messagingFactories = messagingFactories;
        }

        public TopicClient Get(Address address)
        {
            var key = address.ToString();
            var buffer = topicClients.GetOrAdd(key, s =>
            {
                var b = new CircularBuffer<TopicClientEntry>(numberOfTopicClientsPerAddress);
                for (var i = 0; i < numberOfTopicClientsPerAddress; i++)
                {
                    var factory = messagingFactories.Get(address);
                    b.Put(new TopicClientEntry
                    {
                        Client = topicClientCreator.Create(address.Queue, factory)
                    });
                }
                return b;
            });

            var entry = buffer.Get();

            if (entry.Client.IsClosed)
            {
                lock (entry.mutex)
                {
                    if (entry.Client.IsClosed)
                    {
                        var factory = messagingFactories.Get(address);
                        entry.Client = topicClientCreator.Create(address.Queue, factory);
                    }
                }
            }

            return entry.Client;

        }

        class TopicClientEntry
        {
            internal Object mutex = new object();
            internal TopicClient Client;
        }
    }
}