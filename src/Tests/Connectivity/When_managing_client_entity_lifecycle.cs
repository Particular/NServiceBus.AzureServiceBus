namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Connectivity
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using TestUtils;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_managing_entity_client_lifecycle
    {
        [Test]
        public void Creates_a_pool_of_clients_per_entity()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var poolSize = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity);

            var creator = new InterceptedMessageReceiverCreator();

            var lifecycleManager = new MessageReceiverLifeCycleManager(creator, settings);

            lifecycleManager.Get("myqueue", AzureServiceBusConnectionString.Value);

            Assert.AreEqual(poolSize, creator.InvocationCount);
        }

        [Test]
        public void Round_robins_across_instances_in_pool()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var creator = new InterceptedMessageReceiverCreator();

            var lifecycleManager = new MessageReceiverLifeCycleManager(creator, settings);

            var poolSize = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity);

            var first = lifecycleManager.Get("myqueue", AzureServiceBusConnectionString.Value);
            var next = first;

            var reuseInPool = false;
            for (var i = 0; i < poolSize - 1; i++)
            {
                var n = lifecycleManager.Get("myqueue", AzureServiceBusConnectionString.Value);
                reuseInPool &= next == n;
                next = n;
            }

            var second = lifecycleManager.Get("myqueue", AzureServiceBusConnectionString.Value);

            Assert.IsFalse(reuseInPool);
            Assert.AreEqual(first, second);
        }

        [Test]
        public void Replaces_receivers_when_closed()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            settings.Set(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity, 1); // pool size of 1 simplifies the test

            var creator = new InterceptedMessageReceiverCreator();

            var lifecycleManager = new MessageReceiverLifeCycleManager(creator, settings);

            var first = (InterceptedMessageReceiver)lifecycleManager.Get("myqueue", AzureServiceBusConnectionString.Value);

            first.Close();

            var second = (InterceptedMessageReceiver)lifecycleManager.Get("myqueue", AzureServiceBusConnectionString.Value);

            Assert.AreEqual(2, creator.InvocationCount);
            Assert.AreNotEqual(first, second);
        }

#pragma warning disable 618
        class InterceptedMessageReceiverCreator : ICreateMessageReceivers
        {

            public int InvocationCount = 0;

            public Task<IMessageReceiver> Create(string entityPath, string namespaceAlias)
            {
                InvocationCount++;
                return Task.FromResult<IMessageReceiver>(new InterceptedMessageReceiver());
            }

        }

        class InterceptedMessageReceiver : IMessageReceiver
        {
            bool isClosed = false;

            public bool IsClosed => isClosed;

            public RetryPolicy RetryPolicy { get; set; }

            public int PrefetchCount { get; set; }

            public ReceiveMode Mode { get; internal set; }
            public void OnMessage(Func<BrokeredMessage, Task> callback, OnMessageOptions options)
            {
                throw new NotImplementedException();
            }

            public Task CloseAsync()
            {
                throw new NotImplementedException();
            }

            public Task CompleteBatchAsync(IEnumerable<Guid> lockTokens)
            {
                throw new NotImplementedException();
            }

            public void Close()
            {
                isClosed = true;
            }
        }
#pragma warning restore 618
    }
}