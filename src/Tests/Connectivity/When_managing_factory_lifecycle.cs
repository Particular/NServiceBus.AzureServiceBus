namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Connectivity
{
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
    public class When_managing_factory_lifecycle
    {
        [Test]
        public void Creates_a_pool_of_factories_for_namespace()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var poolSize = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace);

            var creator = new InterceptedFactoryCreator();

            var lifecycleManager = new MessagingFactoryLifeCycleManager(creator, settings);

            lifecycleManager.Get(AzureServiceBusConnectionString.Value);

            Assert.AreEqual(poolSize, creator.InvocationCount);
        }

        [Test]
        public void Round_robins_across_instances_in_pool()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var creator = new InterceptedFactoryCreator();

            var lifecycleManager = new MessagingFactoryLifeCycleManager(creator, settings);

            var poolSize = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace);

            var first = lifecycleManager.Get(AzureServiceBusConnectionString.Value);
            var next = first;

            var reuseInPool = false;
            for (var i = 0; i < poolSize-1; i++)
            {
                var n = lifecycleManager.Get(AzureServiceBusConnectionString.Value);
                reuseInPool &= next == n;
                next = n;
            }

            var second = lifecycleManager.Get(AzureServiceBusConnectionString.Value);

            Assert.IsFalse(reuseInPool);
            Assert.AreEqual(first, second);
        }

        [Test]
        public void Replaces_factories_when_closed()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            settings.Set(WellKnownConfigurationKeys.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace, 1); // pool size of 1 simplifies the test

            var creator = new InterceptedFactoryCreator();

            var lifecycleManager = new MessagingFactoryLifeCycleManager(creator, settings);

            var first = (InterceptedFactory)lifecycleManager.Get(AzureServiceBusConnectionString.Value);

            first.Close();

            var second = (InterceptedFactory)lifecycleManager.Get(AzureServiceBusConnectionString.Value);

            Assert.AreEqual(2, creator.InvocationCount);
            Assert.AreNotEqual(first, second);
        }

        class InterceptedFactoryCreator : ICreateMessagingFactoriesInternal
        {
            public int InvocationCount = 0;

            public IMessagingFactoryInternal Create(string namespaceName)
            {
                InvocationCount++;

                return new InterceptedFactory();
            }
        }

        class InterceptedFactory : IMessagingFactoryInternal
        {
            bool isClosed;

            public bool IsClosed => isClosed;

            public void Close()
            {
                isClosed = true;
            }

            public RetryPolicy RetryPolicy
            {
                get { throw new System.NotImplementedException(); }
                set { throw new System.NotImplementedException(); }
            }

            public Task<IMessageReceiverInternal> CreateMessageReceiver(string entitypath, ReceiveMode receiveMode)
            {
                throw new System.NotImplementedException();
            }

            public Task<IMessageSenderInternal> CreateMessageSender(string entitypath)
            {
                throw new System.NotImplementedException();
            }

            public Task<IMessageSenderInternal> CreateMessageSender(string entitypath, string viaEntityPath)
            {
                throw new System.NotImplementedException();
            }

            public Task CloseAsync()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}