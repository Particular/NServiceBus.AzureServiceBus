namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Creation
{
    using System;
    using System.Linq;
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
    public class When_creating_subscription
    {
        const string topicPath = "topic";
        static SubscriptionMetadata metadata = new SubscriptionMetadata { Description = "eventname" };
        const string sqlFilter = "1=1";

        [OneTimeSetUp]
        public void TopicSetup()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            if (!namespaceManager.TopicExists(topicPath).Result)
            {
                namespaceManager.CreateTopic(new TopicDescription(topicPath)).GetAwaiter().GetResult();
            }
        }

        [OneTimeTearDown]
        public void TopicCleanUp()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            namespaceManager.DeleteTopic(topicPath).GetAwaiter().GetResult();
        }

        [Test]
        public async Task Should_not_create_subscription_when_topology_creation_is_turned_off()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            settings.Set(WellKnownConfigurationKeys.Core.CreateTopology, false);

            const string subscriptionName = "mysubscription1";
            //make sure there is no leftover from previous test
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));

            var creator = new AzureServiceBusSubscriptionCreator(settings);

            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager);

            Assert.IsFalse(await namespaceManager.TopicExists(subscriptionName));
        }

        [Test]
        public async Task Should_properly_set_use_subscription_description_defaults_if_user_does_not_provide_topic_description_values()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var creator = new AzureServiceBusSubscriptionCreator(settings);
            const string subscriptionName = "sub1";
            var subscriptionDescription = await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager);

            Assert.IsTrue(await namespaceManager.SubscriptionExists(topicPath, subscriptionName));
            Assert.AreEqual(TimeSpan.MaxValue, subscriptionDescription.AutoDeleteOnIdle);
            Assert.AreEqual(TimeSpan.MaxValue, subscriptionDescription.DefaultMessageTimeToLive);
            Assert.AreEqual(TimeSpan.FromMilliseconds(30000), subscriptionDescription.LockDuration);
            Assert.True(subscriptionDescription.EnableBatchedOperations);
            Assert.IsFalse(subscriptionDescription.EnableDeadLetteringOnFilterEvaluationExceptions);
            Assert.IsFalse(subscriptionDescription.EnableDeadLetteringOnMessageExpiration);
            Assert.IsFalse(subscriptionDescription.RequiresSession);
            Assert.AreEqual(10, subscriptionDescription.MaxDeliveryCount);
            Assert.IsNull(subscriptionDescription.ForwardDeadLetteredMessagesTo);
            Assert.IsNull(subscriptionDescription.ForwardTo);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }


        [Test]
        public async Task Should_properly_set_use_subscription_description_provided_by_user()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            const string subscriptionName = "sub2";
            var subscriptionDescription = new SubscriptionDescription(topicPath, subscriptionName)
            {
                LockDuration = TimeSpan.FromMinutes(3)
            };

            extensions.Subscriptions().DescriptionFactory((x, y, z) => subscriptionDescription);

            var creator = new AzureServiceBusSubscriptionCreator(settings);

            var foundDescription = await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager);

            Assert.IsTrue(await namespaceManager.SubscriptionExists(topicPath, subscriptionName));
            Assert.AreEqual(subscriptionDescription, foundDescription);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_properly_set_AutoDeleteOnIdle_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var autoDeleteTime = TimeSpan.FromDays(1);
            extensions.Subscriptions().AutoDeleteOnIdle(autoDeleteTime);

            var creator = new AzureServiceBusSubscriptionCreator(settings);

            const string subscriptionName = "sub3";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager);
            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.AreEqual(autoDeleteTime, foundDescription.AutoDeleteOnIdle);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_properly_set_DefaultMessageTimeToLive_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var timeToLive = TimeSpan.FromDays(10);
            extensions.Subscriptions().DefaultMessageTimeToLive(timeToLive);

            var creator = new AzureServiceBusSubscriptionCreator(settings);

            const string subscriptionName = "sub4";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.AreEqual(timeToLive, foundDescription.DefaultMessageTimeToLive);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_properly_set_EnableBatchedOperations_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Subscriptions().EnableBatchedOperations(false);

            var creator = new AzureServiceBusSubscriptionCreator(settings);

            const string subscriptionName = "sub5";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.IsFalse(foundDescription.EnableBatchedOperations);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_properly_set_EnableDeadLetteringOnFilterEvaluationExceptions_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Subscriptions().EnableDeadLetteringOnFilterEvaluationExceptions(true);

            var creator = new AzureServiceBusSubscriptionCreator(settings);

            const string subscriptionName = "sub6";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.IsTrue(foundDescription.EnableDeadLetteringOnFilterEvaluationExceptions);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_properly_set_EnableDeadLetteringOnMessageExpiration_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Subscriptions().EnableDeadLetteringOnMessageExpiration(true);

            var creator = new AzureServiceBusSubscriptionCreator(settings);

            const string subscriptionName = "sub7";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.IsTrue(foundDescription.EnableDeadLetteringOnMessageExpiration);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_properly_set_LockDuration_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var lockDuration = TimeSpan.FromMinutes(2);
            extensions.Subscriptions().LockDuration(lockDuration);

            var creator = new AzureServiceBusSubscriptionCreator(settings);

            const string subscriptionName = "sub8";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.AreEqual(lockDuration, foundDescription.LockDuration);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_properly_set_MaxDeliveryCount_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            const int deliveryCount = 10;
            extensions.Subscriptions().MaxDeliveryCount(deliveryCount);

            var creator = new AzureServiceBusSubscriptionCreator(settings);

            const string subscriptionName = "sub9";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.AreEqual(deliveryCount, foundDescription.MaxDeliveryCount);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_set_forwarding_to_an_explicitly_provided_forwardto()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var queueCreator = new AzureServiceBusQueueCreator(new DefaultConfigurationValues().Apply(new SettingsHolder()));
            var queueToForwardTo = await queueCreator.Create("forwardto", namespaceManager);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var creator = new AzureServiceBusSubscriptionCreator(settings);

            const string subscriptionName = "sub15";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, queueToForwardTo.Path);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.That(foundDescription.ForwardTo.EndsWith(queueToForwardTo.Path));

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
            await namespaceManager.DeleteQueue(queueToForwardTo.Path);
        }

        [Test]
        public async Task Should_properly_set_ForwardDeadLetteredMessagesTo_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var topicCreator = new AzureServiceBusTopicCreator(new DefaultConfigurationValues().Apply(new SettingsHolder()));
            var topicToForwardTo = await topicCreator.Create("topic2forward2", namespaceManager);


            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Subscriptions().ForwardDeadLetteredMessagesTo(topicToForwardTo.Path);

            var creator = new AzureServiceBusSubscriptionCreator(settings);

            const string subscriptionName = "sub13";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.That(foundDescription.ForwardDeadLetteredMessagesTo.EndsWith(topicToForwardTo.Path));

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
            await namespaceManager.DeleteTopic(topicToForwardTo.Path);
        }

        [Test]
        public async Task Should_properly_set_ForwardDeadLetteredMessagesTo_on_the_created_entity_that_qualifies_the_condition()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var topicCreator = new AzureServiceBusTopicCreator(new DefaultConfigurationValues().Apply(new SettingsHolder()));
            var topicToForwardTo = await topicCreator.Create("topic2forward2", namespaceManager);


            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Subscriptions().ForwardDeadLetteredMessagesTo(subName => subName == "sub14", topicToForwardTo.Path);

            var creator = new AzureServiceBusSubscriptionCreator(settings);

            const string subscriptionName = "sub14";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.That(foundDescription.ForwardDeadLetteredMessagesTo.EndsWith(topicToForwardTo.Path));

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
            await namespaceManager.DeleteTopic(topicToForwardTo.Path);
        }

        [Test]
        public async Task Should_create_subscription_with_sql_filter()
        {
            const string subscriptionName = "SomeEvent";
            const string filter = @"[NServiceBus.EnclosedMessageTypes] LIKE 'Test.SomeEvent%'"
                                + " OR [NServiceBus.EnclosedMessageTypes] LIKE '%Test.SomeEvent%'"
                                + " OR [NServiceBus.EnclosedMessageTypes] LIKE '%Test.SomeEvent'"
                                + " OR [NServiceBus.EnclosedMessageTypes] = 'Test.SomeEvent'";

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));


            var creator = new AzureServiceBusSubscriptionCreator(settings);
            var subscriptionDescription = await creator.Create(topicPath, subscriptionName, metadata, filter, namespaceManager);
            var rules = await namespaceManager.GetRules(subscriptionDescription);
            var foundFilter = rules.First().Filter as SqlFilter;


            Assert.IsTrue(rules.Count() == 1, "Subscription should only have 1 rule");
            Assert.AreEqual(filter, foundFilter.SqlExpression, "Rule was expected to have a specific SQL filter, but it didn't");

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_store_event_original_name_as_usermetadata()
        {
            const string subscriptionName = "sub16";

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var creator = new AzureServiceBusSubscriptionCreator(settings);
            var subscriptionDescription = await creator.Create(topicPath, subscriptionName, new SubscriptionMetadata { Description = "very.logn.name.of.an.event.that.would.exceed.subscription.length" }, sqlFilter, namespaceManager);

            Assert.IsTrue(await namespaceManager.SubscriptionExists(topicPath, subscriptionName));
            Assert.AreEqual("very.logn.name.of.an.event.that.would.exceed.subscription.length", subscriptionDescription.UserMetadata);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }


        [Test]
        public async Task Should_be_able_to_update_an_existing_subscription_with_new_property_values()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.CreateTopic(new TopicDescription("sometopic1"));
            await namespaceManager.CreateSubscription(new SubscriptionDescription("sometopic1", "existingsubscription1"), sqlFilter);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Subscriptions().DescriptionFactory((topic, subName, readOnlySettings) => new SubscriptionDescription(topic, subName)
            {
                AutoDeleteOnIdle = TimeSpan.FromMinutes(100),
                EnableDeadLetteringOnMessageExpiration = true,
            });

            var creator = new AzureServiceBusSubscriptionCreator(settings);
            await creator.Create("sometopic1", "existingsubscription1", metadata, sqlFilter, namespaceManager);

            var subscriptionDescription = await namespaceManager.GetSubscription("sometopic1", "existingsubscription1");
            Assert.AreEqual(TimeSpan.FromMinutes(100), subscriptionDescription.AutoDeleteOnIdle);

            //cleanup
            await namespaceManager.DeleteTopic("sometopic1");
        }

        [Test]
        public async Task Should_be_able_to_update_an_existing_subscription_with_new_property_values_without_failing_on_readonly_properties()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.CreateTopic(new TopicDescription("sometopic2"));
            await namespaceManager.CreateSubscription(new SubscriptionDescription("sometopic2", "existingsubscription2")
            {
                EnableDeadLetteringOnFilterEvaluationExceptions = true,
                RequiresSession = true,
            }, "1=1");

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Subscriptions().DescriptionFactory((topic, sub, readOnlySettings) => new SubscriptionDescription(topic, sub)
            {
                EnableDeadLetteringOnFilterEvaluationExceptions = false,
                RequiresSession = false
            });

            var creator = new AzureServiceBusSubscriptionCreator(settings);
            Assert.ThrowsAsync<ArgumentException>(async () => await creator.Create("sometopic2", "existingsubscription2", metadata, sqlFilter, namespaceManager));

            //cleanup
            await namespaceManager.DeleteTopic("sometopic2");
        }
    }
}