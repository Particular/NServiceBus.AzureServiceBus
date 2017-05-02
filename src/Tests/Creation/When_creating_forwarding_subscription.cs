namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Creation
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using Configuration;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using TestUtils;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_forwarding_subscription
    {
        const string topicPath = "bundle-x";
        const string forwardToQueue = "forwardToQueue";
        const string sqlFilter = "1=1";
        static SubscriptionMetadataInternal metadata = new ForwardingTopologySubscriptionMetadata
        {
            Description = "endpoint blah",
            NamespaceInfo = new RuntimeNamespaceInfo("name", AzureServiceBusConnectionString.Value),
            SubscribedEventFullName = "event.full.name",
            SubscriptionNameBasedOnEventWithNamespace = "sha1.of.event.full.name"
        };

        [OneTimeSetUp]
        public void TopicSetup()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            if (!namespaceManager.TopicExists(topicPath).GetAwaiter().GetResult())
            {
                namespaceManager.CreateTopic(new TopicDescription(topicPath)).GetAwaiter().GetResult();
            }
            if (!namespaceManager.QueueExists(forwardToQueue).GetAwaiter().GetResult())
            {
                namespaceManager.CreateQueue(new QueueDescription(forwardToQueue)).GetAwaiter().GetResult();
            }

            namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Fallback));
            if (!namespaceManager.TopicExists(topicPath).GetAwaiter().GetResult())
            {
                namespaceManager.CreateTopic(new TopicDescription(topicPath)).GetAwaiter().GetResult();
            }
            if (!namespaceManager.QueueExists(forwardToQueue).GetAwaiter().GetResult())
            {
                namespaceManager.CreateQueue(new QueueDescription(forwardToQueue)).GetAwaiter().GetResult();
            }
        }

        [OneTimeTearDown]
        public void TopicCleanUp()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            namespaceManager.DeleteTopic(topicPath).GetAwaiter().GetResult();
            namespaceManager.DeleteQueue(forwardToQueue).GetAwaiter().GetResult();

            namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            namespaceManager.DeleteTopic(topicPath).GetAwaiter().GetResult();
            namespaceManager.DeleteQueue(forwardToQueue).GetAwaiter().GetResult();
        }

        [Test]
        public async Task Should_properly_set_use_subscription_description_defaults_if_user_does_not_provide_topic_description_values()
        {
            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var creator = new AzureServiceBusForwardingSubscriptionCreator(new TopologySubscriptionSettings(), settings);
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
            Assert.AreEqual(10, subscriptionDescription.MaxDeliveryCount);
            Assert.IsNull(subscriptionDescription.ForwardDeadLetteredMessagesTo);
            Assert.That(subscriptionDescription.ForwardTo, Does.EndWith(forwardToQueue));

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }


        [Test]
        public async Task Should_properly_set_use_subscription_description_provided_by_user()
        {
            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            const string subscriptionName = "endpoint2";

            var userCustomizationsWhereInvoked = false;
            extensions.Subscriptions().DescriptionFactory(_ =>
            {
                userCustomizationsWhereInvoked = true;
            });

            var creator = new AzureServiceBusForwardingSubscriptionCreator(topology.Settings.SubscriptionSettings, settings);

            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            Assert.IsTrue(await namespaceManager.SubscriptionExists(topicPath, subscriptionName));
            Assert.IsTrue(userCustomizationsWhereInvoked);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_properly_set_AutoDeleteOnIdle_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var autoDeleteTime = TimeSpan.FromDays(1);
            extensions.UseForwardingTopology().Subscriptions().AutoDeleteOnIdle(autoDeleteTime);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings.Get<ITopologyInternal>().Settings.SubscriptionSettings, settings);

            const string subscriptionName = "endpoint3";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);
            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            // TODO: remove when ASB bug is fixed
            if (foundDescription.AutoDeleteOnIdle == TimeSpan.MaxValue)
            {
                Assert.Inconclusive("Microsoft ASB bug. Pending response from ASB group: https://www.yammer.com/azureadvisors/#/Threads/show?threadId=654972562");
            }
            Assert.AreEqual(autoDeleteTime, foundDescription.AutoDeleteOnIdle);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_properly_set_DefaultMessageTimeToLive_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var timeToLive = TimeSpan.FromDays(10);
            extensions.UseForwardingTopology().Subscriptions().DefaultMessageTimeToLive(timeToLive);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings.Get<ITopologyInternal>().Settings.SubscriptionSettings, settings);

            const string subscriptionName = "endpoint4";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.AreEqual(timeToLive, foundDescription.DefaultMessageTimeToLive);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_properly_set_EnableBatchedOperations_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.UseForwardingTopology().Subscriptions().EnableBatchedOperations(false);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings.Get<ITopologyInternal>().Settings.SubscriptionSettings, settings);

            const string subscriptionName = "endpoint5";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.IsFalse(foundDescription.EnableBatchedOperations);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_properly_set_EnableDeadLetteringOnFilterEvaluationExceptions_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.UseForwardingTopology().Subscriptions().EnableDeadLetteringOnFilterEvaluationExceptions(true);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings.Get<ITopologyInternal>().Settings.SubscriptionSettings, settings);

            const string subscriptionName = "endpoint6";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.IsTrue(foundDescription.EnableDeadLetteringOnFilterEvaluationExceptions);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_properly_set_EnableDeadLetteringOnMessageExpiration_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.UseForwardingTopology().Subscriptions().EnableDeadLetteringOnMessageExpiration(true);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings.Get<ITopologyInternal>().Settings.SubscriptionSettings, settings);

            const string subscriptionName = "endpoint7";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.IsTrue(foundDescription.EnableDeadLetteringOnMessageExpiration);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_properly_set_LockDuration_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var lockDuration = TimeSpan.FromMinutes(2);
            extensions.UseForwardingTopology().Subscriptions().LockDuration(lockDuration);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings.Get<ITopologyInternal>().Settings.SubscriptionSettings, settings);

            const string subscriptionName = "endpoint8";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.AreEqual(lockDuration, foundDescription.LockDuration);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_properly_set_MaxDeliveryCount_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            const int deliveryCount = 10;
            extensions.UseForwardingTopology().Subscriptions().MaxDeliveryCount(deliveryCount);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings.Get<ITopologyInternal>().Settings.SubscriptionSettings, settings);

            const string subscriptionName = "endpoint9";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.AreEqual(deliveryCount, foundDescription.MaxDeliveryCount);

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
        }

        [Test]
        public async Task Should_set_forwarding_to_an_explicitly_provided_forwardto()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var queueCreator = new AzureServiceBusQueueCreator(new TopologyQueueSettings(), DefaultConfigurationValues.Apply(new SettingsHolder()));
            var queueToForwardTo = await queueCreator.Create("forwardto", namespaceManager);

            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var topology = new FakeTopology(settings);
            var creator = new AzureServiceBusForwardingSubscriptionCreator(topology.Settings.SubscriptionSettings, settings);

            const string subscriptionName = "endpoint15";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, queueToForwardTo.Path);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.That(foundDescription.ForwardTo.EndsWith(queueToForwardTo.Path));

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
            await namespaceManager.DeleteQueue(queueToForwardTo.Path);
        }

        [Test]
        public async Task Should_properly_set_ForwardDeadLetteredMessagesTo_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var topicCreator = new AzureServiceBusTopicCreator(new TopologyTopicSettings());
            var topicToForwardTo = await topicCreator.Create("topic2forward2", namespaceManager);


            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.UseForwardingTopology().Subscriptions().ForwardDeadLetteredMessagesTo(topicToForwardTo.Path);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings.Get<ITopologyInternal>().Settings.SubscriptionSettings, settings);

            const string subscriptionName = "endpoint13";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.That(foundDescription.ForwardDeadLetteredMessagesTo.EndsWith(topicToForwardTo.Path));

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
            await namespaceManager.DeleteTopic(topicToForwardTo.Path);
        }

        [Test]
        public async Task Should_properly_set_ForwardDeadLetteredMessagesTo_on_the_created_entity_that_qualifies_the_condition()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var topicCreator = new AzureServiceBusTopicCreator(new TopologyTopicSettings());
            var notUsedEntity = await topicCreator.Create("topic2forward2", namespaceManager);


            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.UseForwardingTopology().Subscriptions().ForwardDeadLetteredMessagesTo(subName => subName == "endpoint14", notUsedEntity.Path);

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings.Get<ITopologyInternal>().Settings.SubscriptionSettings, settings);

            const string subscriptionName = "endpoint14";
            await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardToQueue);

            var foundDescription = await namespaceManager.GetSubscription(topicPath, subscriptionName);

            Assert.That(foundDescription.ForwardDeadLetteredMessagesTo.EndsWith(notUsedEntity.Path));

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriptionName));
            await namespaceManager.DeleteTopic(notUsedEntity.Path);
        }

        [Test]
        public async Task Should_create_subscription_with_sql_filter()
        {
            const string subscriber = "subscriber";
            const string filter = @"[NServiceBus.EnclosedMessageTypes] LIKE 'Test.SomeEvent%'"
                                + " OR [NServiceBus.EnclosedMessageTypes] LIKE '%Test.SomeEvent%'"
                                + " OR [NServiceBus.EnclosedMessageTypes] LIKE '%Test.SomeEvent'"
                                + " OR [NServiceBus.EnclosedMessageTypes] = 'Test.SomeEvent'";

            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));


            var creator = new AzureServiceBusForwardingSubscriptionCreator(new TopologySubscriptionSettings(), settings);
            var subscriptionDescription = await creator.Create(topicPath, subscriber, metadata, filter, namespaceManager, forwardToQueue);
            var rules = await namespaceManager.GetRules(subscriptionDescription);
            var foundFilter = rules.First().Filter as SqlFilter;


            Assert.IsTrue(rules.Count() == 1, "Subscription should only have 1 rule");
            Assert.AreEqual(filter, foundFilter.SqlExpression, "Rule was expected to have a specific SQL filter, but it didn't");

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriber));
        }


        [Test]
        public async Task Should_be_able_to_update_an_existing_subscription_with_new_property_values()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.CreateSubscription(new SubscriptionDescription(topicPath, "existingendpoint1"), sqlFilter);

            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseForwardingTopology().Subscriptions().DescriptionFactory(description =>
            {
                description.LockDuration = TimeSpan.FromMinutes(5);
                description.EnableDeadLetteringOnMessageExpiration = true;
            });

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings.Get<ITopologyInternal>().Settings.SubscriptionSettings, settings);
            await creator.Create(topicPath, "existingendpoint1", metadata, sqlFilter, namespaceManager, forwardToQueue);

            var subscriptionDescription = await namespaceManager.GetSubscription(topicPath, "existingendpoint1");
            Assert.AreEqual(TimeSpan.FromMinutes(5), subscriptionDescription.LockDuration);
            Assert.IsTrue(subscriptionDescription.EnableDeadLetteringOnMessageExpiration);
        }

        [Test]
        public async Task Should_be_able_to_update_an_existing_subscription_with_new_property_values_without_failing_on_readonly_properties()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.CreateSubscription(new SubscriptionDescription(topicPath, "existingendpoint2")
            {
                EnableDeadLetteringOnFilterEvaluationExceptions = true,
                RequiresSession = true
            }, "1=1");

            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseForwardingTopology().Subscriptions().DescriptionFactory(description =>
            {
                description.EnableDeadLetteringOnFilterEvaluationExceptions = false;
                description.RequiresSession = false;
            });

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings.Get<ITopologyInternal>().Settings.SubscriptionSettings, settings);
            Assert.ThrowsAsync<ArgumentException>(async () => await creator.Create(topicPath, "existingendpoint2", metadata, sqlFilter, namespaceManager, forwardToQueue));
        }

        [Test]
        public async Task Should_be_idempotent_when_creating_a_subscription()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseForwardingTopology().Subscriptions().DescriptionFactory(description =>
            {
                description.MaxDeliveryCount = 100;
                description.EnableDeadLetteringOnMessageExpiration = true;
            });

            var creator = new AzureServiceBusForwardingSubscriptionCreator(settings.Get<ITopologyInternal>().Settings.SubscriptionSettings, settings);
            await creator.Create(topicPath, "someendpoint", metadata, sqlFilter, namespaceManager, forwardToQueue);
            await creator.Create(topicPath, "someendpoint", metadata, sqlFilter, namespaceManager, forwardToQueue);

            var rules = await namespaceManager.GetRules(new SubscriptionDescription(topicPath, "someendpoint"));
            Assert.AreEqual(1, rules.Count());
        }

        [Test]
        public async Task Should_be_able_to_create_partitioned_topic_with_multiple_rules()
        {
            const string subscriber = "subscriber";
            const string filter1 = @"[x] LIKE 'x%'";
            const string filter2 = @"[y] LIKE 'y%'";
            var metadata2 = new ForwardingTopologySubscriptionMetadata
            {
                Description = "endpoint blah",
                NamespaceInfo = new RuntimeNamespaceInfo("name", AzureServiceBusConnectionString.Value),
                SubscribedEventFullName = "event2.full.name",
                SubscriptionNameBasedOnEventWithNamespace = "sha1.of.event2.full.name"
            };


            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseForwardingTopology().Topics().EnablePartitioning(true);
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var creator = new AzureServiceBusForwardingSubscriptionCreator(new TopologySubscriptionSettings(), settings);
            // add subscription with one rule
            await creator.Create(topicPath, subscriber, metadata, filter1, namespaceManager, forwardToQueue);
            // add additional rule to the same subscription
            var subscriptionDescription = await creator.Create(topicPath, subscriber, metadata2, filter2, namespaceManager, forwardToQueue);
            var rules = await namespaceManager.GetRules(subscriptionDescription);

            Assert.That(rules.Count(), Is.EqualTo(2), "Subscription didn't have correct number of rules");

            await namespaceManager.DeleteSubscription(new SubscriptionDescription(topicPath, subscriber));
        }

        [Test]
        public async Task Should_create_subscription_on_multiple_namespaces()
        {
            const string subscriber = "MultipleNamespaceSubscriber";

            var namespaceManager1 = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            var namespaceManager2 = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Fallback));

            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());

            var creator = new AzureServiceBusForwardingSubscriptionCreator(new TopologySubscriptionSettings
            {
                DescriptionCustomizer = description =>
                {
                    description.MaxDeliveryCount = 100;
                    description.EnableDeadLetteringOnMessageExpiration = true;
                }
            }, settings);

            await creator.Create(topicPath, subscriber, metadata, sqlFilter, namespaceManager1, forwardToQueue);
            await creator.Create(topicPath, subscriber, metadata, sqlFilter, namespaceManager2, forwardToQueue);


            Assert.IsTrue(await namespaceManager1.SubscriptionExists(topicPath, subscriber), "Subscription on Value namespace was not created.");
            Assert.IsTrue(await namespaceManager2.SubscriptionExists(topicPath, subscriber), "Subscription on Fallback namespace was not created.");
        }
    }
}