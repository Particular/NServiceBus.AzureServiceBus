namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Creation
{
    using System;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using FakeItEasy;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using TestUtils;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;

#pragma warning disable 618
    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_topics
    {
        [Test]
        public async Task Should_use_topic_description_defaults_if_user_does_not_provide_topic_description_values()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            const string topicPath = "mytopic2";
            await namespaceManager.DeleteTopic(topicPath);

            var creator = new AzureServiceBusTopicCreator(settings);
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
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            const string topicPath = "mytopic3";
            await namespaceManager.DeleteTopic(topicPath);

            var topicDescriptionToUse = new TopicDescription(topicPath)
            {
                AutoDeleteOnIdle = TimeSpan.MaxValue
            };

            extensions.Topics().DescriptionFactory((path, s) => topicDescriptionToUse);

            var creator = new AzureServiceBusTopicCreator(settings);

            var description = await creator.Create(topicPath, namespaceManager);

            Assert.IsTrue(await namespaceManager.TopicExists(topicPath));
            Assert.AreEqual(topicDescriptionToUse, description);
        }

        [Test]
        public async Task Should_set_AutoDeleteOnIdle_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var autoDeleteTime = TimeSpan.FromDays(1);
            extensions.Topics().AutoDeleteOnIdle(autoDeleteTime);

            var creator = new AzureServiceBusTopicCreator(settings);

            const string topicPath = "mytopic4";
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);
            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.AreEqual(autoDeleteTime, foundTopic.AutoDeleteOnIdle);
        }

        [Test]
        public async Task Should_set_DefaultMessageTimeToLive_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var timeToLive = TimeSpan.FromDays(1);
            extensions.Topics().DefaultMessageTimeToLive(timeToLive);

            var creator = new AzureServiceBusTopicCreator(settings);
            const string topicPath = "mytopic5";
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.AreEqual(timeToLive, foundTopic.DefaultMessageTimeToLive);
        }

        [Test]
        public async Task Should_set_DuplicateDetectionHistoryTimeWindow_on_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var duplicateDetectionTime = TimeSpan.FromDays(1);
            extensions.Topics().DuplicateDetectionHistoryTimeWindow(duplicateDetectionTime);

            var creator = new AzureServiceBusTopicCreator(settings);
            const string topicPath = "mytopic6";
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.AreEqual(duplicateDetectionTime, foundTopic.DuplicateDetectionHistoryTimeWindow);
        }

        [Test]
        public async Task Should_set_EnableBatchedOperations_on_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topics().EnableBatchedOperations(false);

            var creator = new AzureServiceBusTopicCreator(settings);
            const string topicPath = "mytopic7";
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.IsFalse(foundTopic.EnableBatchedOperations);
        }

        [Test]
        public async Task Should_set_EnableFilteringMessagesBeforePublishing_on_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topics().EnableFilteringMessagesBeforePublishing(true);

            var creator = new AzureServiceBusTopicCreator(settings);
            const string topicPath = "mytopic8";
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.IsTrue(foundTopic.EnableFilteringMessagesBeforePublishing);
        }

        [Test]
        public async Task Should_set_EnablePartitioning_on_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            const string topicPath = "mytopic9";

            //clean up before test starts
            await namespaceManager.DeleteTopic(topicPath);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topics().EnablePartitioning(true);

            var creator = new AzureServiceBusTopicCreator(settings);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.IsTrue(foundTopic.EnablePartitioning);
        }

        [Test]
        public async Task Should_set_MaxSizeInMegabytes_on_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topics().MaxSizeInMegabytes(SizeInMegabytes.Size4096);

            var creator = new AzureServiceBusTopicCreator(settings);
            const string topicPath = "mytopic10";
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.AreEqual(4096, foundTopic.MaxSizeInMegabytes);
        }

        [Test]
        public async Task Should_set_RequiresDuplicateDetection_on_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topics().RequiresDuplicateDetection(true);

            var creator = new AzureServiceBusTopicCreator(settings);
            const string topicPath = "mytopic11";
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.IsTrue(foundTopic.RequiresDuplicateDetection);
        }

        [Test]
        public async Task Should_set_SupportOrdering_on_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topics().SupportOrdering(true);

            var creator = new AzureServiceBusTopicCreator(settings);
            const string topicPath = "mytopic12";
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.IsTrue(foundTopic.SupportOrdering);
        }

        [Test]
        public async Task Should_set_correct_defaults()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            var creator = new AzureServiceBusTopicCreator(new DefaultConfigurationValues().Apply(new SettingsHolder()));
            const string topicPath = "mytopic13";
            await namespaceManager.DeleteTopic(topicPath);

            await creator.Create(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopic(topicPath);

            Assert.AreEqual(TimeSpan.MaxValue, foundTopic.AutoDeleteOnIdle);
            Assert.AreEqual(TimeSpan.MaxValue, foundTopic.DefaultMessageTimeToLive);
            Assert.AreEqual(TimeSpan.FromMilliseconds(600000), foundTopic.DuplicateDetectionHistoryTimeWindow);
            Assert.IsTrue(foundTopic.EnableBatchedOperations);
            Assert.IsFalse(foundTopic.EnableExpress);
            Assert.IsFalse(foundTopic.EnableFilteringMessagesBeforePublishing);
            Assert.IsFalse(foundTopic.EnablePartitioning);
            Assert.AreEqual(1024, foundTopic.MaxSizeInMegabytes);
            Assert.IsFalse(foundTopic.RequiresDuplicateDetection);
            Assert.IsFalse(foundTopic.SupportOrdering);
        }


        [Test]
        public async Task Should_not_throw_when_another_node_creates_the_same_topic_first()
        {
            const string topicPath = "testtopic";

            var namespaceManager = A.Fake<INamespaceManager>();
            A.CallTo(() => namespaceManager.TopicExists(topicPath)).Returns(Task.FromResult(false));

            var topicCreationThrewException = false;
            A.CallTo(() => namespaceManager.CreateTopic(A<TopicDescription>.Ignored))
                .Invokes(() => topicCreationThrewException = true)
                .Throws(() => new MessagingEntityAlreadyExistsException("blah"));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var creator = new AzureServiceBusTopicCreator(settings);

            await creator.Create(topicPath, namespaceManager);

            Assert.IsTrue(topicCreationThrewException);
        }

        [Test]
        public void Should_throw_TimeoutException_if_creation_of_entity_timed_out_and_topic_was_not_created()
        {
            var namespaceManager = A.Fake<INamespaceManager>();
            A.CallTo(() => namespaceManager.TopicExists(A<string>.Ignored)).Returns(Task.FromResult(false));
            A.CallTo(() => namespaceManager.CreateTopic(A<TopicDescription>.Ignored)).Throws<TimeoutException>();

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var creator = new AzureServiceBusTopicCreator(settings);

            Assert.ThrowsAsync<TimeoutException>(async () => await creator.Create("faketopic", namespaceManager));
        }

        [Test]
        public Task Should_not_throw_TimeoutException_if_creation_of_entity_timed_out_and_topic_was_created()
        {
            var namespaceManager = A.Fake<INamespaceManager>();
            A.CallTo(() => namespaceManager.TopicExists(A<string>.Ignored)).ReturnsNextFromSequence(Task.FromResult(false), Task.FromResult(true));
            A.CallTo(() => namespaceManager.CreateTopic(A<TopicDescription>.Ignored)).Throws<TimeoutException>();

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var creator = new AzureServiceBusTopicCreator(settings);

            return creator.Create("faketopic", namespaceManager);
        }

        [Test]
        public void Should_throw_for_MessagingException_that_is_not_transient()
        {
            var namespaceManager = A.Fake<INamespaceManager>();
            A.CallTo(() => namespaceManager.TopicExists(A<string>.Ignored)).Throws(new MessagingException("boom", false, new Exception("wrapped")));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var creator = new AzureServiceBusTopicCreator(settings);

            Assert.ThrowsAsync<MessagingException>(async () => await creator.Create("faketopic", namespaceManager));
        }

        [Test]
        public async Task Should_be_able_to_update_an_existing_topic_with_new_property_values()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteTopic("existingtopic1");

            await namespaceManager.CreateTopic(new TopicDescription("existingtopic1"));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Topics().DescriptionFactory((topicPath, readOnlySettings) => new TopicDescription(topicPath)
            {
                AutoDeleteOnIdle = TimeSpan.FromMinutes(100),
                EnableExpress = true
            });

            var creator = new AzureServiceBusTopicCreator(settings);
            await creator.Create("existingtopic1", namespaceManager);

            var topicDescription = await namespaceManager.GetTopic("existingtopic1");
            Assert.AreEqual(TimeSpan.FromMinutes(100), topicDescription.AutoDeleteOnIdle);
        }

        [Test]
        public async Task Should_be_able_to_update_an_existing_topic_with_new_property_values_without_failing_on_readonly_properties()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            await namespaceManager.DeleteTopic("existingtopic2");

            await namespaceManager.CreateTopic(new TopicDescription("existingtopic2")
            {
                MaxSizeInMegabytes = 2048,
                RequiresDuplicateDetection = true,
                EnablePartitioning = true
            });

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Topics().DescriptionFactory((queuePath, readOnlySettings) => new TopicDescription(queuePath)
            {
                MaxSizeInMegabytes = 1024,
                RequiresDuplicateDetection = false,
                EnablePartitioning = false
            });

            var creator = new AzureServiceBusTopicCreator(settings);
            Assert.ThrowsAsync<ArgumentException>(async () => await creator.Create("existingtopic2", namespaceManager));
        }
    }
}