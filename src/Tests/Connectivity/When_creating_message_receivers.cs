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

#pragma warning disable 618
    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_message_receivers
    {
        [Test]
        public async Task Delegates_creation_to_messaging_factory()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var factory = new InterceptedMessagingFactory();

            var creator = new MessageReceiverCreator(new InterceptedMessagingFactoryFactory(factory), settings);

            var receiver = await creator.Create("myqueue", AzureServiceBusConnectionString.Value);

            Assert.IsTrue(factory.IsInvoked);
            Assert.IsInstanceOf<IMessageReceiver>(receiver);
        }

        [Test]
        public async Task Applies_user_defined_connectivity_settings()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.MessageReceivers()
                      .PrefetchCount(1000)
                      .RetryPolicy(RetryPolicy.NoRetry)
                      .ReceiveMode(ReceiveMode.ReceiveAndDelete);

            var factory = new InterceptedMessagingFactory();

            var creator = new MessageReceiverCreator(new InterceptedMessagingFactoryFactory(factory), settings);

            var receiver = await creator.Create("myqueue", AzureServiceBusConnectionString.Value);

            Assert.AreEqual(ReceiveMode.ReceiveAndDelete, receiver.Mode);
            Assert.IsInstanceOf<NoRetry>(receiver.RetryPolicy);
            Assert.AreEqual(1000, receiver.PrefetchCount);
        }

#pragma warning disable 618
        class InterceptedMessagingFactoryFactory : IManageMessagingFactoryLifeCycle
        {
            readonly IMessagingFactory factory;

            public InterceptedMessagingFactoryFactory(IMessagingFactory factory)
            {
                this.factory = factory;
            }

            public IMessagingFactory Get(string namespaceName)
            {
                return factory;
            }

            public Task CloseAll()
            {
                throw new NotImplementedException();
            }
        }

        class InterceptedMessagingFactory : IMessagingFactory
        {
            public bool IsInvoked;

            public bool IsClosed
            {
                get { throw new NotImplementedException(); }
            }

            public RetryPolicy RetryPolicy
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public Task<IMessageReceiver> CreateMessageReceiver(string entitypath, ReceiveMode receiveMode)
            {
                IsInvoked = true;

                return Task.FromResult<IMessageReceiver>(new FakeMessageReceiver() { Mode = receiveMode });
            }

            public Task<IMessageSender> CreateMessageSender(string entitypath)
            {
                throw new NotImplementedException();
            }

            public Task<IMessageSender> CreateMessageSender(string entitypath, string viaEntityPath)
            {
                throw new NotImplementedException();
            }

            public Task CloseAsync()
            {
                throw new NotImplementedException();
            }
        }

        class FakeMessageReceiver : IMessageReceiver
        {
            public bool IsClosed => false;

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
        }
#pragma warning restore 618
    }
}