namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using Microsoft.ServiceBus;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_managing_namespace_manager_lifecycle
    {
        [Test]
        public void Requests_creation_of_new_manager_for_namespace_initially()
        {
            var creator = new InterceptingCreator();

            var lifecycleManager = new NamespaceManagerLifeCycleManager(creator);

            lifecycleManager.Get(AzureServiceBusConnectionString.Value);

            Assert.IsTrue(creator.HasBeenInvoked);
        }

        [Test]
        public void Caches_single_manager_for_reuse()
        {
            var creator = new InterceptingCreator();

            var lifecycleManager = new NamespaceManagerLifeCycleManager(creator);

            var first = lifecycleManager.Get(AzureServiceBusConnectionString.Value);
            var second = lifecycleManager.Get(AzureServiceBusConnectionString.Value);

            Assert.AreEqual(1, creator.InvocationCount);
            Assert.AreEqual(first, second);
        }
        
        class InterceptingCreator : ICreateNamespaceManagers
        {
            public bool HasBeenInvoked;
            public int InvocationCount = 0;

            public INamespaceManager Create(string connectionstring)
            {
                HasBeenInvoked = true;
                InvocationCount++;

                return new InterceptedManager();
            }
        }

        class InterceptedManager : INamespaceManager
        {
            public NamespaceManagerSettings Settings
            {
                get { throw new NotImplementedException(); }
            }

            public Uri Address
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}