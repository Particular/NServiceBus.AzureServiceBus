namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus.Creation;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_topics
    {
        Action cleanup_action;

        [TearDown]
        public void CleanUp()
        {
            cleanup_action();
        }

        [Test]
        public async Task Should_not_create_topics_when_core_turned_topology_creation_off()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            settings.Set(WellKnownConfigurationKeys.Core.CreateTopology, false);

            //make sure there is no leftover from previous test
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));
            const string topicPath = "mytopic1";
            await namespaceManager.DeleteTopicAsync(topicPath);

            var creator = new AzureServiceBusTopicCreator(settings);

            await creator.CreateAsync(topicPath, namespaceManager);

            Assert.IsFalse(await namespaceManager.TopicExistsAsync(topicPath));

            cleanup_action = () => { };
        }

        [Test]
        public async Task Should_use_topic_description_defaults_if_user_does_not_provide_topic_description_values()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            const string topicPath = "mytopic2";

            var creator = new AzureServiceBusTopicCreator(settings);
            var topicDescription = await creator.CreateAsync(topicPath, namespaceManager);

            Assert.IsTrue(await namespaceManager.TopicExistsAsync(topicPath));
            Assert.AreEqual(TimeSpan.MaxValue, topicDescription.AutoDeleteOnIdle);
            Assert.AreEqual(TimeSpan.MaxValue, topicDescription.DefaultMessageTimeToLive);
            Assert.AreEqual(TimeSpan.FromMilliseconds(600000), topicDescription.DuplicateDetectionHistoryTimeWindow);
            Assert.IsFalse(topicDescription.EnableBatchedOperations);
            Assert.IsFalse(topicDescription.EnableExpress);
            Assert.IsFalse(topicDescription.EnableFilteringMessagesBeforePublishing);
            Assert.IsFalse(topicDescription.EnablePartitioning);
            Assert.AreEqual(1024, topicDescription.MaxSizeInMegabytes);
            Assert.IsFalse(topicDescription.RequiresDuplicateDetection);
            Assert.IsFalse(topicDescription.SupportOrdering);

            cleanup_action = () => namespaceManager.DeleteTopicAsync(topicPath);
        }


        [Test]
        public async Task Should_use_topic_description_provided_by_user()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            const string topicPath = "mytopic3";
            var topicDescriptionToUse = new TopicDescription(topicPath)
            {
                AutoDeleteOnIdle = TimeSpan.MaxValue
            };
            
            extensions.Topology().Resources().Topics().DescriptionFactory((path, s) => topicDescriptionToUse);

            var creator = new AzureServiceBusTopicCreator(settings);

            var description = await creator.CreateAsync(topicPath, namespaceManager);

            Assert.IsTrue(await namespaceManager.TopicExistsAsync(topicPath));
            Assert.AreEqual(topicDescriptionToUse, description);

            cleanup_action = () => namespaceManager.DeleteTopicAsync(topicPath);
        }

        [Test]
        public async Task Should_set_AutoDeleteOnIdle_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var autoDeleteTime = TimeSpan.FromDays(1);
            extensions.Topology().Resources().Topics().AutoDeleteOnIdle(autoDeleteTime);

            var creator = new AzureServiceBusTopicCreator(settings);

            const string topicPath = "mytopic4";
            await creator.CreateAsync(topicPath, namespaceManager);
            var foundTopic = await namespaceManager.GetTopicAsync(topicPath);

            Assert.AreEqual(autoDeleteTime, foundTopic.AutoDeleteOnIdle);

            cleanup_action = () => namespaceManager.DeleteTopicAsync(topicPath);
        }

        [Test]
        public async Task Should_set_DefaultMessageTimeToLive_on_the_created_entity()
        {
            var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var timeToLive = TimeSpan.FromDays(1);
            extensions.Topology().Resources().Topics().DefaultMessageTimeToLive(timeToLive);

            var creator = new AzureServiceBusTopicCreator(settings);
            const string topicPath = "mytopic5";
            await creator.CreateAsync(topicPath, namespaceManager);

            var foundTopic = await namespaceManager.GetTopicAsync(topicPath);

            Assert.AreEqual(timeToLive, foundTopic.DefaultMessageTimeToLive);

            cleanup_action = () => namespaceManager.DeleteTopicAsync(topicPath);
        }

//        [Test]
//        public async Task Should_not_not_hang_on_Result_invocation_in_a_catch_block()
//        {
//            var namespaceManager = A.Fake<INamespaceManager>();
//            A.CallTo(() => namespaceManager.TopicExistsAsync(A<string>.Ignored)).Returns(Task.FromResult(false)).Once();
//            A.CallTo(() => namespaceManager.CreateTopicAsync(A<TopicDescription>.Ignored)).Throws<TimeoutException>();
//            A.CallTo(() => namespaceManager.TopicExistsAsync(A<string>.Ignored)).Returns(Task.FromResult(true)).Once();
//
//            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
//            var creator = new AzureServiceBusTopicCreator(settings);
//
//            await creator.CreateAsync("testtopic", namespaceManager);
//        }
    }
}