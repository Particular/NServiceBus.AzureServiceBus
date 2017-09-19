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
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_message_senders
    {
        [Test]
        public async Task Delegates_creation_to_messaging_factory()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());

            var factory = new InterceptedMessagingFactory();

            var creator = new MessageSenderCreator(new InterceptedMessagingFactoryFactory(factory), settings);

            var sender = await creator.Create("myqueue", null, AzureServiceBusConnectionString.Value);

            Assert.IsTrue(factory.IsInvoked);
            Assert.IsInstanceOf<IMessageSenderInternal>(sender);
        }

        [Test]
        public async Task Applies_user_defined_connectivity_settings()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());

            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.MessageSenders()
                      .RetryPolicy(RetryPolicy.NoRetry);

            var factory = new InterceptedMessagingFactory();

            var creator = new MessageSenderCreator(new InterceptedMessagingFactoryFactory(factory), settings);

            var sender = await creator.Create("myqueue", null, AzureServiceBusConnectionString.Value);

            Assert.IsInstanceOf<NoRetry>(sender.RetryPolicy);
        }

        class InterceptedMessagingFactoryFactory : IManageMessagingFactoryLifeCycleInternal
        {
            readonly IMessagingFactoryInternal factory;

            public InterceptedMessagingFactoryFactory(IMessagingFactoryInternal factory)
            {
                this.factory = factory;
            }

            public IMessagingFactoryInternal Get(string namespaceName)
            {
                return factory;
            }

            public Task CloseAll()
            {
                throw new NotImplementedException();
            }
        }

        class InterceptedMessagingFactory : IMessagingFactoryInternal
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

            public Task<IMessageReceiverInternal> CreateMessageReceiver(string entitypath, ReceiveMode receiveMode)
            {
                throw new NotImplementedException();
            }

            public Task<IMessageSenderInternal> CreateMessageSender(string entitypath)
            {
                IsInvoked = true;

                return Task.FromResult<IMessageSenderInternal>(new FakeMessageSender());
            }

            public Task<IMessageSenderInternal> CreateMessageSender(string entitypath, string viaEntityPath)
            {
                throw new NotImplementedException();
            }

            public Task CloseAsync()
            {
                throw new NotImplementedException();
            }
        }

        class FakeMessageSender : IMessageSenderInternal
        {
            public bool IsClosed => false;

            public RetryPolicy RetryPolicy
            {
                get; set;
            }

            public Task Send(BrokeredMessage message)
            {
                throw new NotImplementedException();
            }

            public Task SendBatch(IEnumerable<BrokeredMessage> messages)
            {
                throw new NotImplementedException();
            }
        }
    }
}