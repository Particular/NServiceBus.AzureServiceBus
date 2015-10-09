namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_message_senders
    {
        [Test]
        public async Task Delegates_creation_to_messaging_factory()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var factory = new InterceptedMessagingFactory();

            var creator = new MessageSenderCreator(new InterceptedMessagingFactoryFactory(factory), settings);

            var sender = await creator.CreateAsync("myqueue", AzureServiceBusConnectionString.Value);

            Assert.IsTrue(factory.IsInvoked);
            Assert.IsInstanceOf<IMessageSender>(sender);
        }

        [Test]
        public async Task Applies_user_defined_connectivity_settings()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Connectivity().MessageSenders()
                      .RetryPolicy(RetryPolicy.NoRetry);

            var factory = new InterceptedMessagingFactory();

            var creator = new MessageSenderCreator(new InterceptedMessagingFactoryFactory(factory), settings);

            var sender = (IMessageSender)await creator.CreateAsync("myqueue", AzureServiceBusConnectionString.Value);

            Assert.IsInstanceOf<NoRetry>(sender.RetryPolicy);
        }

        class InterceptedMessagingFactoryFactory : IManageMessagingFactoryLifeCycle
        {
            readonly IMessagingFactory factory;

            public InterceptedMessagingFactoryFactory(IMessagingFactory factory)
            {
                this.factory = factory;
            }

            public IMessagingFactory Get(string @namespace)
            {
                return factory;
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

            public Task<IMessageReceiver> CreateMessageReceiverAsync(string entitypath, ReceiveMode receiveMode)
            {
                throw new NotImplementedException();
            }

            public Task<IMessageSender> CreateMessageSenderAsync(string entitypath)
            {
                IsInvoked = true;

                return Task.FromResult<IMessageSender>(new FakeMessageSender());
            }

            public Task<IMessageSender> CreateMessageSenderAsync(string entitypath, string viaEntityPath)
            {
                throw new NotImplementedException();
            }
        }

        class FakeMessageSender : IMessageSender
        {
            public bool IsClosed
            {
                get { return false; }
            }

            public RetryPolicy RetryPolicy
            {
                get; set;
            }

            public Task SendAsync(BrokeredMessage message)
            {
                throw new NotImplementedException();
            }

            public Task SendBatchAsync(IEnumerable<BrokeredMessage> messages)
            {
                throw new NotImplementedException();
            }
        }
    }
}