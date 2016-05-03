namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Creation
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using TestUtils;
    using AzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_subscription_backward_compatible_with_v6
    {
        const string topicPath = "topicV6";

        [TestFixtureSetUp]
        public void TopicSetup()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            if (!namespaceManager.TopicExists(topicPath).Result)
            {
                namespaceManager.CreateTopic(new TopicDescription(topicPath)).Wait();
            }
        }

        [TestFixtureTearDown]
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