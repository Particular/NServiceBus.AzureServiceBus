namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
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

            public Task<bool> HasManageRights()
            {
                throw new NotImplementedException(); 
            }

            public Task CreateQueue(QueueDescription description)
            {
                throw new NotImplementedException();
            }

            public Task UpdateQueue(QueueDescription description)
            {
                throw new NotImplementedException();
            }

            public Task DeleteQueue(string path)
            {
                throw new NotImplementedException();
            }

            public Task<QueueDescription> GetQueue(string path)
            {
                throw new NotImplementedException();
            }

            public Task<bool> QueueExists(string path)
            {
                throw new NotImplementedException();
            }

            public Task<TopicDescription> CreateTopic(TopicDescription topicDescription)
            {
                throw new NotImplementedException();
            }

            public Task<TopicDescription> GetTopic(string path)
            {
                throw new NotImplementedException();
            }

            public Task<TopicDescription> UpdateTopic(TopicDescription topicDescription)
            {
                throw new NotImplementedException();
            }

            public Task<bool> TopicExists(string path)
            {
                throw new NotImplementedException();
            }

            public Task DeleteTopic(string path)
            {
                throw new NotImplementedException();
            }

            public Task<bool> SubscriptionExists(string topicPath, string subscriptionName)
            {
                throw new NotImplementedException();
            }

            public Task<SubscriptionDescription> CreateSubscription(SubscriptionDescription subscriptionDescription, string sqlFilter)
            {
                throw new NotImplementedException();
            }

            public Task<SubscriptionDescription> GetSubscription(string topicPath, string subscriptionName)
            {
                throw new NotImplementedException();
            }

            public Task<SubscriptionDescription> UpdateSubscription(SubscriptionDescription subscriptionDescription)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<RuleDescription>> GetRules(SubscriptionDescription subscriptionDescription)
            {
                throw new NotImplementedException();
            }
        }
    }
}