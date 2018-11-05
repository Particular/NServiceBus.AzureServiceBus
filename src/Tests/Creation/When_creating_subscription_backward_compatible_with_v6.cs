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

        [OneTimeSetUp]
        public void TopicSetup()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            if (!namespaceManager.TopicExists(topicPath).Result)
            {
                namespaceManager.CreateTopic(new TopicDescription(topicPath)).Wait();
            }
        }

        [OneTimeTearDown]
        public void TopicCleanUp()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            namespaceManager.DeleteTopic(topicPath).Wait();
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