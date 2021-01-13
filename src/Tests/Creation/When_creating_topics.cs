namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Creation
{
    using System;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using TestUtils;
    using Transport.AzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_topics
    {
        [Test]
        public async Task Should_use_topic_description_defaults_if_user_does_not_provide_topic_description_values()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            const string topicPath = "mytopic2";
            await namespaceManager.DeleteTopic(topicPath);

            var creator = new AzureServiceBusTopicCreator(new TopologyTopicSettings());
            var topicDescription = await creator.Create(topicPath, namespaceManager);

            Assert.IsTrue(await namespaceManager.TopicExists(topicPath));
            Assert.AreEqual(TimeSpan.MaxValue, topicDescription.AutoDeleteOnIdle);
            Assert.AreEqual(TimeSpan.MaxValue, topicDescription.DefaultMessageTimeToLive);
            Assert.AreEqual(TimeSpan.FromMilliseconds(600000), topicDescription.DuplicateDetectionHistoryTimeWindow);
            Assert.IsTrue(topicDescription.EnableBatchedOperations);
            Assert.IsFalse(topicDescription.EnableExpress);
            Assert.IsFalse(topicDescription.EnableFilteringMessagesBeforePublishing);
            Assert.IsFalse(topicDescription.EnablePartitioning);
            Assert.AreEqual(1024, topicDescription.MaxSizeInMegabytes);
            Assert.IsFalse(topicDescription.RequiresDuplicateDetection);
            Assert.IsFalse(topicDescription.SupportOrdering);
        }

        [Test]
        public async Task Should_use_topic_description_provided_by_user()
        {
            const string topicPath = "mytopic3";
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteTopic(topicPath);

            var userProvidedTopicDescriptionWasUsed = false;

            var topologyTopicSettings = new TopologyTopicSettings
            {
                DescriptionCustomizer = td => { userProvidedTopicDescriptionWasUsed = true; }
            };
            var creator = new AzureServiceBusTopicCreator(topologyTopicSettings);

            await creator.Create(topicPath, namespaceManager);

            Assert.IsTrue(await namespaceManager.TopicExists(topicPath));
            Assert.IsTrue(userProvidedTopicDescriptionWasUsed);
        }

        [Test]
        public async Task Should_set_AutoDeleteOnIdle_on_the_created_entity()
        {
            var autoDeleteTime = TimeSpan.FromDays(1);

            var topologyTopicSettings = new TopologyTopicSettings
            {
                AutoDeleteOnIdle = autoDeleteTime
            };
            var creator = new AzureServiceBusTopicCreator(topologyTopicSettings);

            const string topicPath = "mytopic4";
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);
            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.AreEqual(autoDeleteTime, foundTopic.AutoDeleteOnIdle);
        }

        [Test]
        public async Task Should_set_DefaultMessageTimeToLive_on_the_created_entity()
        {
            var timeToLive = TimeSpan.FromDays(1);
            var topologyTopicSettings = new TopologyTopicSettings
            {
                DefaultMessageTimeToLive = timeToLive
            };

            var creator = new AzureServiceBusTopicCreator(topologyTopicSettings);
            const string topicPath = "mytopic5";
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.AreEqual(timeToLive, foundTopic.DefaultMessageTimeToLive);
        }

        [Test]
        public async Task Should_set_DuplicateDetectionHistoryTimeWindow_on_created_entity()
        {
            var duplicateDetectionTime = TimeSpan.FromDays(1);

            var topologyTopicSettings = new TopologyTopicSettings
            {
                DuplicateDetectionHistoryTimeWindow = duplicateDetectionTime
            };
            var creator = new AzureServiceBusTopicCreator(topologyTopicSettings);
            const string topicPath = "mytopic6";
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.AreEqual(duplicateDetectionTime, foundTopic.DuplicateDetectionHistoryTimeWindow);
        }

        [Test]
        public async Task Should_set_EnableBatchedOperations_on_created_entity()
        {
            var topologyTopicSettings = new TopologyTopicSettings
            {
                EnableBatchedOperations = false
            };
            var creator = new AzureServiceBusTopicCreator(topologyTopicSettings);
            const string topicPath = "mytopic7";
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.IsFalse(foundTopic.EnableBatchedOperations);
        }

        [Test]
        public async Task Should_set_EnableFilteringMessagesBeforePublishing_on_created_entity()
        {
            var topologyTopicSettings = new TopologyTopicSettings
            {
                EnableFilteringMessagesBeforePublishing = true
            };
            var creator = new AzureServiceBusTopicCreator(topologyTopicSettings);
            const string topicPath = "mytopic8";
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.IsTrue(foundTopic.EnableFilteringMessagesBeforePublishing);
        }

        [Test]
        public async Task Should_set_EnablePartitioning_on_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            const string topicPath = "mytopic9";

            //clean up before test starts
            await namespaceManager.DeleteTopic(topicPath);

            var topologyTopicSettings = new TopologyTopicSettings
            {
                EnablePartitioning = true
            };
            var creator = new AzureServiceBusTopicCreator(topologyTopicSettings);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.IsTrue(foundTopic.EnablePartitioning);
        }

        [Test]
        public async Task Should_set_MaxSizeInMegabytes_on_created_entity()
        {
            var topologyTopicSettings = new TopologyTopicSettings
            {
                MaxSizeInMegabytes = SizeInMegabytes.Size4096
            };
            var creator = new AzureServiceBusTopicCreator(topologyTopicSettings);
            const string topicPath = "mytopic10";
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.AreEqual(4096, foundTopic.MaxSizeInMegabytes);
        }

        [Test]
        public async Task Should_set_RequiresDuplicateDetection_on_created_entity()
        {
            var topologyTopicSettings = new TopologyTopicSettings
            {
                RequiresDuplicateDetection = true
            };
            var creator = new AzureServiceBusTopicCreator(topologyTopicSettings);
            const string topicPath = "mytopic11";
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.IsTrue(foundTopic.RequiresDuplicateDetection);
        }

        [Test]
        public async Task Should_set_SupportOrdering_on_created_entity()
        {
            var topologyTopicSettings = new TopologyTopicSettings
            {
                SupportOrdering = true
            };
            var creator = new AzureServiceBusTopicCreator(topologyTopicSettings);
            const string topicPath = "mytopic12";
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.IsTrue(foundTopic.SupportOrdering);
        }

        [Test]
        public async Task Should_set_correct_defaults()
        {
            var creator = new AzureServiceBusTopicCreator(new TopologyTopicSettings());
            const string topicPath = "mytopic13";
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.AreEqual(TimeSpan.MaxValue, foundTopic.AutoDeleteOnIdle);
            Assert.AreEqual(TimeSpan.MaxValue, foundTopic.DefaultMessageTimeToLive);
            Assert.AreEqual(TimeSpan.FromMinutes(10), foundTopic.DuplicateDetectionHistoryTimeWindow);
            Assert.IsTrue(foundTopic.EnableBatchedOperations);
            Assert.IsFalse(foundTopic.EnableExpress);
            Assert.IsFalse(foundTopic.EnableFilteringMessagesBeforePublishing);
            Assert.IsFalse(foundTopic.EnablePartitioning);
            Assert.AreEqual((long)SizeInMegabytes.Size1024, foundTopic.MaxSizeInMegabytes);
            Assert.IsFalse(foundTopic.RequiresDuplicateDetection);
            Assert.IsFalse(foundTopic.SupportOrdering);
        }


        [Test]
        public async Task Should_not_throw_when_another_node_creates_the_same_topic_first()
        {
            const string topicPath = "testtopic";

            var namespaceManager = A.Fake<INamespaceManagerInternal>();
            A.CallTo(() => namespaceManager.TopicExists(topicPath)).Returns(Task.FromResult(false));

            var topicCreationThrewException = false;
            A.CallTo(() => namespaceManager.CreateTopic(A<TopicDescription>.Ignored))
                .Invokes(() => topicCreationThrewException = true)
                .Throws(() => new MessagingEntityAlreadyExistsException("blah"));

            var creator = new AzureServiceBusTopicCreator(new TopologyTopicSettings());

            await creator.Create(topicPath, namespaceManager);

            Assert.IsTrue(topicCreationThrewException);
        }

        [Test]
        public void Should_throw_TimeoutException_if_creation_of_entity_timed_out_and_topic_was_not_created()
        {
            var namespaceManager = A.Fake<INamespaceManagerInternal>();
            A.CallTo(() => namespaceManager.TopicExists(A<string>.Ignored)).Returns(Task.FromResult(false));
            A.CallTo(() => namespaceManager.CreateTopic(A<TopicDescription>.Ignored)).Throws<TimeoutException>();

            var creator = new AzureServiceBusTopicCreator(new TopologyTopicSettings());

            Assert.ThrowsAsync<TimeoutException>(async () => await creator.Create("faketopic", namespaceManager));
        }

        [Test]
        public Task Should_not_throw_TimeoutException_if_creation_of_entity_timed_out_and_topic_was_created()
        {
            var namespaceManager = A.Fake<INamespaceManagerInternal>();
            A.CallTo(() => namespaceManager.TopicExists(A<string>.Ignored)).ReturnsNextFromSequence(Task.FromResult(false), Task.FromResult(true));
            A.CallTo(() => namespaceManager.CreateTopic(A<TopicDescription>.Ignored)).Throws<TimeoutException>();

            var creator = new AzureServiceBusTopicCreator(new TopologyTopicSettings());

            return creator.Create("faketopic", namespaceManager);
        }

        [Test]
        public void Should_throw_for_MessagingException_that_is_not_transient()
        {
            var namespaceManager = A.Fake<INamespaceManagerInternal>();
            A.CallTo(() => namespaceManager.TopicExists(A<string>.Ignored)).Throws(new MessagingException("boom", false, new Exception("wrapped")));

            var creator = new AzureServiceBusTopicCreator(new TopologyTopicSettings());

            Assert.ThrowsAsync<MessagingException>(async () => await creator.Create("faketopic", namespaceManager));
        }

        [Test]
        public async Task Should_be_able_to_update_an_existing_topic_with_new_property_values()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteTopic("existingtopic1");

            await namespaceManager.CreateTopic(new TopicDescription("existingtopic1"));

            var topologyTopicSettings = new TopologyTopicSettings
            {
                AutoDeleteOnIdle = TimeSpan.FromMinutes(100),
                DefaultMessageTimeToLive = TimeSpan.FromMinutes(5)
            };
            var creator = new AzureServiceBusTopicCreator(topologyTopicSettings);
            await creator.Create("existingtopic1", namespaceManager);

            var topicDescription = await namespaceManager.GetTopic("existingtopic1");
            Assert.AreEqual(TimeSpan.FromMinutes(100), topicDescription.AutoDeleteOnIdle);
        }

        [Test]
        public async Task Should_be_able_to_update_an_existing_topic_with_new_property_values_without_failing_on_readonly_properties()
        {
            var namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteTopic("existingtopic2");
            await namespaceManager.CreateTopic(new TopicDescription("existingtopic2")
            {
                MaxSizeInMegabytes = SizeInMegabytes.Size2048,
                RequiresDuplicateDetection = true,
                EnablePartitioning = true
            });

            var topicDescription = await namespaceManager.GetTopic("existingtopic2");

            // partitioned topics will have a size that is 16x the requested max
            Assert.AreEqual(2048 * 16, topicDescription.MaxSizeInMegabytes);
            Assert.IsTrue(topicDescription.EnablePartitioning);
            Assert.IsTrue(topicDescription.RequiresDuplicateDetection);

            var topologyTopicSettings = new TopologyTopicSettings
            {
                MaxSizeInMegabytes = SizeInMegabytes.Size3072,
                RequiresDuplicateDetection = false,
                EnablePartitioning = false
            };
            var creator = new AzureServiceBusTopicCreator(topologyTopicSettings);
            await creator.Create("existingtopic2", namespaceManager);

            topicDescription = await namespaceManager.GetTopic("existingtopic2");
            Assert.AreEqual(3072 * 16, topicDescription.MaxSizeInMegabytes);
            Assert.IsTrue(topicDescription.EnablePartitioning);
            Assert.IsTrue(topicDescription.RequiresDuplicateDetection);
        }

        [Test]
        public async Task Should_create_topic_on_multiple_namespaces()
        {
            var namespaceManager1 = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            var namespaceManager2 = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Fallback));
            const string topicPath = "topic-caching-key";
            await namespaceManager1.DeleteTopic(topicPath);
            await namespaceManager2.DeleteTopic(topicPath);

            var creator = new AzureServiceBusTopicCreator(new TopologyTopicSettings());
            await creator.Create(topicPath, namespaceManager1);
            await creator.Create(topicPath, namespaceManager2);

            Assert.IsTrue(await namespaceManager1.TopicExists(topicPath));
            Assert.IsTrue(await namespaceManager2.TopicExists(topicPath));
        }
    }
}