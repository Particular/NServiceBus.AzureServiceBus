namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Creation
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests.TestUtils;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Settings;
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
            await namespaceManager.CreateSubscription(new SubscriptionDescription(topicPath, typeof(SomeEvent).Name), 
                new SqlSubscriptionFilter(typeof(Creation.SomeEvent)).Serialize());

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var creator = new AzureServiceBusSubscriptionCreatorV6(settings);
            var shortedSubscriptionName = MD5DeterministicNameBuilder.Build(typeof(SomeEvent).FullName);
            var metadata = new SubscriptionMetadata
            {
                SubscriptionNameBasedOnEventWithNamespace = shortedSubscriptionName,
                Description = Guid.NewGuid().ToString()
            };
            await creator.Create(topicPath, typeof(SomeEvent).Name, metadata, new SqlSubscriptionFilter(typeof(SomeEvent)).Serialize(), namespaceManager);

            var subscriptionDescription = await namespaceManager.GetSubscription(topicPath, shortedSubscriptionName);
            Assert.AreEqual(metadata.Description, subscriptionDescription.UserMetadata);
            Assert.AreEqual(metadata.SubscriptionNameBasedOnEventWithNamespace, subscriptionDescription.Name);
        }

        class SomeEvent {}
    }

    class SomeEvent { }
}