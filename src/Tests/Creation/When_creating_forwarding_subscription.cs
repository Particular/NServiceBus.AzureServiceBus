namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Creation
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using TestUtils;
    using AzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_forwarding_subscription
    {
        const string topicPath = "bundle-x";
        const string forwardToQueue = "forwardToQueue";
        const string sqlFilter = "1=1";
        static SubscriptionMetadata metadata = new ForwardingTopologySubscriptionMetadata
        {
            Description = "endpoint blah",
            NamespaceInfo = new RuntimeNamespaceInfo("name", AzureServiceBusConnectionString.Value),
            SubscribedEventFullName = "event.full.name",
            SubscriptionNameBasedOnEventWithNamespace = "sha1.of.event.full.name"
        };

        [TestFixtureSetUp]
        public void TopicSetup()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            if (!namespaceManager.TopicExists(topicPath).GetAwaiter().GetResult())
            {
                namespaceManager.CreateTopic(new TopicDescription(topicPath)).GetAwaiter().GetResult();
            }
            if (!namespaceManager.QueueExists(forwardToQueue).GetAwaiter().GetResult())
            {
                namespaceManager.CreateQueue(new QueueDescription(forwardToQueue)).GetAwaiter().GetResult();
            }
        }

        [TestFixtureTearDown]
        public void TopicCleanUp()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            namespaceManager.DeleteTopic(topicPath).GetAwaiter().GetResult();
            namespaceManager.DeleteQueue(forwardToQueue).GetAwaiter().GetResult();
        }

        [Test]
        public async Task Should_not_create_subscription_when_topology_creation_is_turned_off()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            settings.Set(WellKnownConfigurationKeys.Core.CreateTopology, false);

            const string subscriptionName = "endpoint0";
            //make sure there is no leftover from previous test
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteSubscriptionAsync(topicPath, subscriptionName);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);

            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            Assert.IsFalse(await namespaceManager.TopicExists(subscriptionName));
        }

        [Test]
        public async Task Should_properly_set_use_subscription_description_defaults_if_user_does_not_provide_topic_description_values()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);
            const string subscriptionName = "endpoint1";
            var subscriptionDescription = await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            Assert.IsTrue(await namespaceManager.SubscriptionExists(topicPath, subscriptionName));
            Assert.AreEqual(TimeSpan.MaxValue, subscriptionDescription.AutoDeleteOnIdle);
            Assert.AreEqual(TimeSpan.MaxValue, subscriptionDescription.DefaultMessageTimeToLive);
            Assert.AreEqual(TimeSpan.FromMilliseconds(30000), subscriptionDescription.LockDuration);
            Assert.True(subscriptionDescription.EnableBatchedOperations);
            Assert.IsFalse(subscriptionDescription.EnableDeadLetteringOnFilterEvaluationExceptions);
            Assert.IsFalse(subscriptionDescription.EnableDeadLetteringOnMessageExpiration);
            Assert.IsFalse(subscriptionDescription.RequiresSession);
            Assert.AreEqual(6, subscriptionDescription.MaxDeliveryCount);
            Assert.IsNull(subscriptionDescription.ForwardDeadLetteredMessagesTo);
            Assert.That(subscriptionDescription.ForwardTo, Is.StringEnding(forwardToQueue));

            await namespaceManager.DeleteSubscriptionAsync(topicPath, subscriptionName);
        }


        [Test]
        public async Task Should_properly_set_use_subscription_description_provided_by_user()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            const string subscriptionName = "endpoint2";
            var subscriptionDescription = new SubscriptionDescription(topicPath, subscriptionName)
            {
                LockDuration = TimeSpan.FromMinutes(3)
            };

            extensions.UseTopology<ForwardingTopology>().Subscriptions().DescriptionFactory((x, y, z) => subscriptionDescription);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);

            var foundDescription = await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            Assert.IsTrue(await namespaceManager.SubscriptionExists(topicPath, subscriptionName));
            Assert.AreEqual(subscriptionDescription, foundDescription);

            await namespaceManager.DeleteSubscriptionAsync(topicPath, subscriptionName);
        }

        [Test]
        public async Task Should_properly_set_AutoDeleteOnIdle_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var autoDeleteTime = TimeSpan.FromDays(1);
            extensions.UseTopology<ForwardingTopology>().Subscriptions().AutoDeleteOnIdle(autoDeleteTime);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);

            const string subscriptionName = "endpoint3";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);
            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            // TODO: remove when ASB bug is fixed
            if (foundDescription.AutoDeleteOnIdle == TimeSpan.MaxValue)
            {
                Assert.Inconclusive("Microsoft ASB bug. Pending response from ASB group: https://www.yammer.com/azureadvisors/#/Threads/show?threadId=654972562");
            }
            Assert.AreEqual(autoDeleteTime, foundDescription.AutoDeleteOnIdle);

            await namespaceManager.DeleteSubscriptionAsync(topicPath, subscriptionName);
        }

        [Test]
        public async Task Should_properly_set_DefaultMessageTimeToLive_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var timeToLive = TimeSpan.FromDays(10);
            extensions.UseTopology<ForwardingTopology>().Subscriptions().DefaultMessageTimeToLive(timeToLive);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);

            const string subscriptionName = "endpoint4";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.AreEqual(timeToLive, foundDescription.DefaultMessageTimeToLive);

            await namespaceManager.DeleteSubscriptionAsync(topicPath, subscriptionName);
        }

        [Test]
        public async Task Should_properly_set_EnableBatchedOperations_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.UseTopology<ForwardingTopology>().Subscriptions().EnableBatchedOperations(false);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);

            const string subscriptionName = "endpoint5";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.IsFalse(foundDescription.EnableBatchedOperations);

            await namespaceManager.DeleteSubscriptionAsync(topicPath, subscriptionName);
        }

        [Test]
        public async Task Should_properly_set_EnableDeadLetteringOnFilterEvaluationExceptions_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.UseTopology<ForwardingTopology>().Subscriptions().EnableDeadLetteringOnFilterEvaluationExceptions(true);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);

            const string subscriptionName = "endpoint6";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.IsTrue(foundDescription.EnableDeadLetteringOnFilterEvaluationExceptions);

            await namespaceManager.DeleteSubscriptionAsync(topicPath, subscriptionName);
        }

        [Test]
        public async Task Should_properly_set_EnableDeadLetteringOnMessageExpiration_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.UseTopology<ForwardingTopology>().Subscriptions().EnableDeadLetteringOnMessageExpiration(true);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);

            const string subscriptionName = "endpoint7";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.IsTrue(foundDescription.EnableDeadLetteringOnMessageExpiration);

            await namespaceManager.DeleteSubscriptionAsync(topicPath, subscriptionName);
        }

        [Test]
        public async Task Should_properly_set_LockDuration_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var lockDuration = TimeSpan.FromMinutes(2);
            extensions.UseTopology<ForwardingTopology>().Subscriptions().LockDuration(lockDuration);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);

            const string subscriptionName = "endpoint8";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.AreEqual(lockDuration, foundDescription.LockDuration);

            await namespaceManager.DeleteSubscriptionAsync(topicPath, subscriptionName);
        }

        [Test]
        public async Task Should_properly_set_MaxDeliveryCount_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            const int deliveryCount = 10;
            extensions.UseTopology<ForwardingTopology>().Subscriptions().MaxDeliveryCount(deliveryCount);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);

            const string subscriptionName = "endpoint9";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.AreEqual(deliveryCount, foundDescription.MaxDeliveryCount);

            await namespaceManager.DeleteSubscriptionAsync(topicPath, subscriptionName);
        }

        [Test]
        public async Task Should_properly_set_RequiresSession_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.UseTopology<ForwardingTopology>().Subscriptions().RequiresSession(true);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);

            const string subscriptionName = "endpoint10";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.IsTrue(foundDescription.RequiresSession);

            await namespaceManager.DeleteSubscriptionAsync(topicPath, subscriptionName);
        }

        [Test]
        public async Task Should_set_forwarding_to_an_explicitly_provided_forwardto()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var queueCreator = new AzureServiceBusQueueCreator(new DefaultConfigurationValues().Apply(new SettingsHolder()));
            var queueToForwardTo = await queueCreator.Create("forwardto", namespaceManager);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);

            const string subscriptionName = "endpoint15";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, queueToForwardTo.Path);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.That(foundDescription.ForwardTo.EndsWith(queueToForwardTo.Path));

            await namespaceManager.DeleteSubscriptionAsync(topicPath, subscriptionName);
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

            extensions.UseTopology<ForwardingTopology>().Subscriptions().ForwardDeadLetteredMessagesTo(topicToForwardTo.Path);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);

            const string subscriptionName = "endpoint13";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.That(foundDescription.ForwardDeadLetteredMessagesTo.EndsWith(topicToForwardTo.Path));

            await namespaceManager.DeleteSubscriptionAsync(topicPath, subscriptionName);
            await namespaceManager.DeleteTopic(topicToForwardTo.Path);
        }

        [Test]
        public async Task Should_properly_set_ForwardDeadLetteredMessagesTo_on_the_created_entity_that_qualifies_the_condition()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var topicCreator = new AzureServiceBusTopicCreator(new DefaultConfigurationValues().Apply(new SettingsHolder()));
            var notUsedEntity = await topicCreator.Create("topic2forward2", namespaceManager);


            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.UseTopology<ForwardingTopology>().Subscriptions().ForwardDeadLetteredMessagesTo(subName => subName == "endpoint14", notUsedEntity.Path);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);

            const string subscriptionName = "endpoint14";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.That(foundDescription.ForwardDeadLetteredMessagesTo.EndsWith(notUsedEntity.Path));

            await namespaceManager.DeleteSubscriptionAsync(topicPath, subscriptionName);
            await namespaceManager.DeleteTopic(notUsedEntity.Path);
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


            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);
            var subscriptionDescription = await creator.Create(topicPath, subscriptionName, metadata, filter, namespaceManager, forwardToQueue);
            var rules = await namespaceManager.GetRules(subscriptionDescription);
            var foundFilter = rules.First().Filter as SqlFilter;


            Assert.IsTrue(rules.Count() == 1, "Subscription should only have 1 rule");
            Assert.AreEqual(filter, foundFilter.SqlExpression, "Rule was expected to have a specific SQL filter, but it didn't");

            await namespaceManager.DeleteSubscriptionAsync(topicPath, subscriptionName);
        }


        [Test]
        public async Task Should_be_able_to_update_an_existing_subscription_with_new_property_values()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.CreateSubscription(new SubscriptionDescription(topicPath, "existingendpoint1"), sqlFilter);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseTopology<ForwardingTopology>().Subscriptions().DescriptionFactory((topic, subName, readOnlySettings) => new SubscriptionDescription(topic, subName)
            {
                MaxDeliveryCount = 100,
                EnableDeadLetteringOnMessageExpiration = true,
            });

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);
            await creator.Create(topicPath, "existingendpoint1", metadata, sqlFilter, namespaceManager, forwardToQueue);

            var subscriptionDescription = await namespaceManager.GetSubscription(topicPath, "existingendpoint1");
            Assert.AreEqual(100, subscriptionDescription.MaxDeliveryCount);
        }

        [Test]
        public async Task Should_be_able_to_update_an_existing_subscription_with_new_property_values_without_failing_on_readonly_properties()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.CreateSubscription(new SubscriptionDescription(topicPath, "existingendpoint2")
            {
                EnableDeadLetteringOnFilterEvaluationExceptions = true,
                RequiresSession = true,
            }, "1=1");

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseTopology<ForwardingTopology>().Subscriptions().DescriptionFactory((topic, sub, readOnlySettings) => new SubscriptionDescription(topic, sub)
            {
                EnableDeadLetteringOnFilterEvaluationExceptions = false,
                RequiresSession = false,
            });

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);
            Assert.Throws<ArgumentException>(async () => await creator.Create(topicPath, "existingendpoint2", metadata, sqlFilter, namespaceManager, forwardToQueue));
        }

        [Test]
        public async Task Should_be_idempotent_when_creating_a_subscription()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseTopology<ForwardingTopology>().Subscriptions().DescriptionFactory((topic, subName, readOnlySettings) => new SubscriptionDescription(topic, subName)
            {
                MaxDeliveryCount = 100,
                EnableDeadLetteringOnMessageExpiration = true,
            });

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings);
            await creator.Create(topicPath, "someendpoint", metadata, sqlFilter, namespaceManager, forwardToQueue);
            await creator.Create(topicPath, "someendpoint", metadata, sqlFilter, namespaceManager, forwardToQueue);

            var rules = await namespaceManager.GetRules(new SubscriptionDescription(topicPath, "someendpoint"));
            Assert.AreEqual(1, rules.Count());
        }
    }
}