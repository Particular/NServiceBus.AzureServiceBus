namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Creation
{
    using System;
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
    public class When_creating_subscription_backward_compatible_with_v6
    {
        const string topicPath = "topicV6";
        const string hierarchyTopicPath = "hierarchy/tenant1/topicV6";

        [OneTimeSetUp]
        public void TopicSetup()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
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
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            namespaceManager.DeleteTopic(topicPath).Wait();
            namespaceManager.DeleteTopic(hierarchyTopicPath).Wait();
        }

        [Test]
        public async Task Should_create_a_subscription_based_on_event_type_full_name_for_an_event_name_reused_across_multiple_namespaces()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.CreateSubscription(new SubscriptionDescription(topicPath, typeof(Ns1.ReusedEvent).Name), new SqlSubscriptionFilter(typeof(Ns1.ReusedEvent)).Serialize());

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var creator = new AzureServiceBusSubscriptionCreatorV6(settings);
            var metadata1 = new SubscriptionMetadata
            {
                SubscriptionNameBasedOnEventWithNamespace = typeof(Ns1.ReusedEvent).FullName,
                Description = Guid.NewGuid().ToString()
            };
            var metadata2 = new SubscriptionMetadata
            {
                SubscriptionNameBasedOnEventWithNamespace = typeof(Ns2.ReusedEvent).FullName,
                Description = Guid.NewGuid().ToString()
            };
            var shortedSubscriptionName = typeof(Ns2.ReusedEvent).FullName;

            await creator.Create(topicPath, typeof(Ns1.ReusedEvent).Name, metadata1, new SqlSubscriptionFilter(typeof(Ns1.ReusedEvent)).Serialize(), namespaceManager);
            await creator.Create(topicPath, typeof(Ns2.ReusedEvent).Name, metadata2, new SqlSubscriptionFilter(typeof(Ns2.ReusedEvent)).Serialize(), namespaceManager);

            var subscriptionDescription = await namespaceManager.GetSubscription(topicPath, shortedSubscriptionName);
            Assert.AreEqual(metadata2.Description, subscriptionDescription.UserMetadata);
            Assert.AreEqual(metadata2.SubscriptionNameBasedOnEventWithNamespace, subscriptionDescription.Name);
            
            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, typeof(Ns1.ReusedEvent).Name));
            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, typeof(Ns2.ReusedEvent).Name));
        }

        [Test]
        public async Task Should_properly_set_ForwardTo_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.CreateSubscription(new SubscriptionDescription(topicPath, typeof(Ns1.ReusedEvent).Name), new SqlSubscriptionFilter(typeof(Ns1.ReusedEvent)).Serialize());

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var topicCreator = new AzureServiceBusTopicCreator(settings);
            var topicToForwardTo = await topicCreator.Create("topic2forward2", namespaceManager);

            var creator = new AzureServiceBusSubscriptionCreatorV6(settings);
            var metadata1 = new SubscriptionMetadata
            {
                SubscriptionNameBasedOnEventWithNamespace = typeof(Ns1.ReusedEvent).FullName,
                Description = Guid.NewGuid().ToString()
            };

            var subscriptionName = typeof(Ns1.ReusedEvent).Name;

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
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.CreateSubscription(new SubscriptionDescription(hierarchyTopicPath, typeof(Ns1.ReusedEvent).Name), new SqlSubscriptionFilter(typeof(Ns1.ReusedEvent)).Serialize());

            var settings = new SettingsHolder();
            var defaults = new DefaultConfigurationValues();
            defaults.Apply(settings);
            
            var topicCreator = new AzureServiceBusTopicCreator(settings);
            var topicToForwardTo = await topicCreator.Create("topic2forward2", namespaceManager);

            var creator = new AzureServiceBusSubscriptionCreatorV6(settings);
            var metadata1 = new SubscriptionMetadata
            {
                SubscriptionNameBasedOnEventWithNamespace = typeof(Ns1.ReusedEvent).FullName,
                Description = Guid.NewGuid().ToString()
            };
            
            var subscriptionName = typeof(Ns1.ReusedEvent).Name;

            await creator.Create(hierarchyTopicPath, subscriptionName, metadata1, new SqlSubscriptionFilter(typeof(Ns1.ReusedEvent)).Serialize(), namespaceManager, topicToForwardTo.Path);
            // create again without forward to
            await creator.Create(hierarchyTopicPath, subscriptionName, metadata1, new SqlSubscriptionFilter(typeof(Ns1.ReusedEvent)).Serialize(), namespaceManager);

            var subscriptionDescription = await namespaceManager.GetSubscription(hierarchyTopicPath, subscriptionName);
            
            Assert.IsNull(subscriptionDescription.ForwardTo);
            
            await namespaceManager.DeleteSubscription(new SubscriptionDescription(hierarchyTopicPath, subscriptionName));
            await namespaceManager.DeleteTopic(topicToForwardTo.Path);
        }
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