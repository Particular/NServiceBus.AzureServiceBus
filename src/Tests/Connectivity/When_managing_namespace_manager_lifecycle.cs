namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
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

            public Task CreateQueueAsync(QueueDescription description)
            {
                throw new NotImplementedException();
            }

            public Task UpdateQueueAsync(QueueDescription description)
            {
                throw new NotImplementedException();
            }

            public Task DeleteQueueAsync(string path)
            {
                throw new NotImplementedException();
            }

            public Task<QueueDescription> GetQueueAsync(string path)
            {
                throw new NotImplementedException();
            }

            public Task<bool> QueueExistsAsync(string path)
            {
                throw new NotImplementedException();
            }

            public Task<TopicDescription> CreateTopicAsync(TopicDescription topicDescription)
            {
                throw new NotImplementedException();
            }

            public Task<TopicDescription> GetTopicAsync(string path)
            {
                throw new NotImplementedException();
            }

            public Task<TopicDescription> UpdateTopicAsync(TopicDescription topicDescription)
            {
                throw new NotImplementedException();
            }

            public Task<bool> TopicExistsAsync(string path)
            {
                throw new NotImplementedException();
            }

            public Task DeleteTopicAsync(string path)
            {
                throw new NotImplementedException();
            }
        }
    }
}