namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_managing_factory_lifecycle
    {
        [Test]
        public void Creates_a_pool_of_factories_for_namespace()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var poolSize = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfMessagingFactoriesPerNamespace);

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

            var poolSize = settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfMessagingFactoriesPerNamespace);

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
            settings.Set(WellKnownConfigurationKeys.Connectivity.NumberOfMessagingFactoriesPerNamespace, 1); // pool size of 1 simplifies the test
            
            var creator = new InterceptedFactoryCreator();

            var lifecycleManager = new MessagingFactoryLifeCycleManager(creator, settings);

            var first = (InterceptedFactory)lifecycleManager.Get(AzureServiceBusConnectionString.Value);

            first.Close();

            var second = (InterceptedFactory)lifecycleManager.Get(AzureServiceBusConnectionString.Value);

            Assert.AreEqual(2, creator.InvocationCount);
            Assert.AreNotEqual(first, second);
        }

        class InterceptedFactoryCreator : ICreateMessagingFactories
        {
            public int InvocationCount = 0;

            public IMessagingFactory Create(string connectionstring)
            {
                InvocationCount++;

                return new InterceptedFactory();
            }
        }

        class InterceptedFactory : IMessagingFactory
        {
            bool _isClosed = false;

            public bool IsClosed
            {
                get { return _isClosed; }
            }
            public void Close()
            {
                _isClosed = true;
            }

            public RetryPolicy RetryPolicy
            {
                get { throw new System.NotImplementedException(); }
                set { throw new System.NotImplementedException(); }
            }

            public Task<IMessageReceiver> CreateMessageReceiverAsync(string entitypath, ReceiveMode receiveMode)
            {
                throw new System.NotImplementedException();
            }

        }
    }
}