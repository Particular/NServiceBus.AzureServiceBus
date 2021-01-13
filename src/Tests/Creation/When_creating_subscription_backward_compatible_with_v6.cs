namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Creation
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Ns3;
    using TestUtils;
    using Transport.AzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_subscription_backward_compatible_with_v6
    {
        const string topicPath = "topicV6";
        const string hierarchyTopicPath = "hierarchy/tenant1/topicV6";

        [OneTimeSetUp]
        public void TopicSetup()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            if (!namespaceManager.TopicExists(topicPath).Result)
            {
                namespaceManager.CreateTopic(new TopicDescription(topicPath)).Wait();
            }

            if (!namespaceManager.TopicExists(hierarchyTopicPath).Result)
            {
                namespaceManager.CreateTopic(new TopicDescription(hierarchyTopicPath)).Wait();
            }
        }

        [OneTimeTearDown]
        public void TopicCleanUp()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);
            namespaceManager.DeleteTopic(topicPath);
            namespaceManager.DeleteTopic(hierarchyTopicPath);
        }

        [Test]
        public async Task Should_create_a_subscription_based_on_event_type_full_name_for_an_event_name_reused_across_multiple_namespaces()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.CreateSubscription(new SubscriptionDescription(topicPath, nameof(Ns1.ReusedEvent)), new SqlSubscriptionFilter(typeof(Ns1.ReusedEvent)).Serialize());

            var creator = new AzureServiceBusSubscriptionCreatorV6(new TopologySubscriptionSettings());
            var metadata1 = new SubscriptionMetadataInternal
            {
                SubscriptionNameBasedOnEventWithNamespace = typeof(Ns1.ReusedEvent).FullName,
                Description = Guid.NewGuid().ToString()
            };
            var metadata2 = new SubscriptionMetadataInternal
            {
                SubscriptionNameBasedOnEventWithNamespace = typeof(Ns2.ReusedEvent).FullName,
                Description = Guid.NewGuid().ToString()
            };
            var shortedSubscriptionName = typeof(Ns2.ReusedEvent).FullName;

            await creator.Create(topicPath, nameof(Ns1.ReusedEvent), metadata1, new SqlSubscriptionFilter(typeof(Ns1.ReusedEvent)).Serialize(), namespaceManager);
            await creator.Create(topicPath, nameof(Ns2.ReusedEvent), metadata2, new SqlSubscriptionFilter(typeof(Ns2.ReusedEvent)).Serialize(), namespaceManager);

            var subscriptionDescription = await namespaceManager.GetSubscription(topicPath, shortedSubscriptionName);
            Assert.AreEqual(metadata2.Description, subscriptionDescription.UserMetadata);
            Assert.AreEqual(metadata2.SubscriptionNameBasedOnEventWithNamespace, subscriptionDescription.Name);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, nameof(Ns1.ReusedEvent)));
            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, nameof(Ns2.ReusedEvent)));
        }

        [Test]
        public async Task Should_properly_set_ForwardTo_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.CreateSubscription(new SubscriptionDescription(topicPath, nameof(Ns1.ReusedEvent)), new SqlSubscriptionFilter(typeof(Ns1.ReusedEvent)).Serialize());

            var topicCreator = new AzureServiceBusTopicCreator(new TopologyTopicSettings());
            var topicToForwardTo = await topicCreator.Create("topic2forward2", namespaceManager);

            var creator = new AzureServiceBusSubscriptionCreatorV6(new TopologySubscriptionSettings());
            var metadata1 = new SubscriptionMetadataInternal
            {
                SubscriptionNameBasedOnEventWithNamespace = typeof(Ns1.ReusedEvent).FullName,
                Description = Guid.NewGuid().ToString()
            };

            var subscriptionName = nameof(Ns1.ReusedEvent);

            await creator.Create(topicPath, subscriptionName, metadata1, new SqlSubscriptionFilter(typeof(Ns1.ReusedEvent)).Serialize(), namespaceManager, topicToForwardTo.Path);
            // create again without forward to
            await creator.Create(topicPath, subscriptionName, metadata1, new SqlSubscriptionFilter(typeof(Ns1.ReusedEvent)).Serialize(), namespaceManager);

            var subscriptionDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.IsNull(subscriptionDescription.ForwardTo);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
            await namespaceManager.DeleteTopic(topicToForwardTo.Path);
        }

        [Test]
        public async Task Should_properly_set_ForwardTo_on_the_created_entity_with_hierarchy()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.CreateSubscription(new SubscriptionDescription(hierarchyTopicPath, nameof(Ns1.ReusedEvent)), new SqlSubscriptionFilter(typeof(Ns1.ReusedEvent)).Serialize());

            var topicCreator = new AzureServiceBusTopicCreator(new TopologyTopicSettings());
            var topicToForwardTo = await topicCreator.Create("topic2forward2", namespaceManager);

            var creator = new AzureServiceBusSubscriptionCreatorV6(new TopologySubscriptionSettings());
            var metadata1 = new SubscriptionMetadataInternal
            {
                SubscriptionNameBasedOnEventWithNamespace = typeof(Ns1.ReusedEvent).FullName,
                Description = Guid.NewGuid().ToString()
            };

            var subscriptionName = nameof(Ns1.ReusedEvent);

            await creator.Create(hierarchyTopicPath, subscriptionName, metadata1, new SqlSubscriptionFilter(typeof(Ns1.ReusedEvent)).Serialize(), namespaceManager, topicToForwardTo.Path);
            // create again without forward to
            await creator.Create(hierarchyTopicPath, subscriptionName, metadata1, new SqlSubscriptionFilter(typeof(Ns1.ReusedEvent)).Serialize(), namespaceManager);

            var subscriptionDescription = await namespaceManager.GetSubscription(hierarchyTopicPath, subscriptionName);

            Assert.IsNull(subscriptionDescription.ForwardTo);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(hierarchyTopicPath, subscriptionName));
            await namespaceManager.DeleteTopic(topicToForwardTo.Path);
        }

        [Test]
        public async Task Should_not_create_create_a_duplicate_subscription__issue_811()
        {
            var nativeManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);
            var namespaceManager = new NamespaceManagerAdapterInternal(nativeManager);

            var topicForTest = $"{topicPath}_issue811";
            try
            {
                if (!await nativeManager.TopicExistsAsync(topicForTest))
                {
                    await nativeManager.CreateTopicAsync(new TopicDescription(topicForTest));
                }

                var subscriptionName = nameof(SomeEvent);

                await namespaceManager.CreateSubscription(new SubscriptionDescription(topicForTest, subscriptionName), new SqlSubscriptionFilter_UsedPriorToVersion9(typeof(SomeEvent)).Serialize());

                var creator = new AzureServiceBusSubscriptionCreatorV6(new TopologySubscriptionSettings());
                var metadata = new SubscriptionMetadataInternal
                {
                    SubscriptionNameBasedOnEventWithNamespace = typeof(SomeEvent).FullName,
                    Description = Guid.NewGuid().ToString()
                };
                var properSqlFilter = new SqlSubscriptionFilter(typeof(SomeEvent)).Serialize();

                await creator.Create(topicForTest, subscriptionName, metadata, properSqlFilter, namespaceManager);

                var foundSubcriptions = await nativeManager.GetSubscriptionsAsync(topicForTest);

                Assert.AreEqual(1, foundSubcriptions.Count());
            }
            finally
            {
                await nativeManager.DeleteTopicAsync(topicForTest);
            }
        }
    }

    class SqlSubscriptionFilter_UsedPriorToVersion9 : IBrokerSideSubscriptionFilterInternal
    {
        public SqlSubscriptionFilter_UsedPriorToVersion9(Type eventType)
        {
            this.eventType = eventType;
        }

        public string Serialize() => string.Format("[{0}] LIKE '{1}%' OR [{0}] LIKE '%{1}%' OR [{0}] LIKE '%{1}' OR [{0}] = '{1}'", Headers.EnclosedMessageTypes, eventType.FullName);

        Type eventType;
    }
}

namespace Ns1
{
    class ReusedEvent { }
}

namespace Ns2
{
    class ReusedEvent { }
}

namespace Ns3
{
    class SomeEvent { }
}