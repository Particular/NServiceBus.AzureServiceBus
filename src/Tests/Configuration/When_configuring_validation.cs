namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_validation
    {
        SettingsHolder settingsHolder;
        TransportExtensions<AzureServiceBusTransport> extensions;

        [SetUp]
        public void SetUp()
        {
            settingsHolder = new SettingsHolder();
            extensions = new TransportExtensions<AzureServiceBusTransport>(settingsHolder);
        }


        [Test]
        public void Should_be_able_to_set_queue_path_maximum_length()
        {
            var validationSettings = extensions.Sanitization().UseQueuePathMaximumLength(10);

            Assert.AreEqual(10, validationSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathMaximumLength));
        }

        [Test]
        public void Should_be_able_to_set_topic_path_maximum_length()
        {
            var validationSettings = extensions.Sanitization().UseTopicPathMaximumLength(20);

            Assert.AreEqual(20, validationSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathMaximumLength));
        }

        [Test]
        public void Should_be_able_to_set_subscription_path_maximum_length()
        {
            var validationSettings = extensions.Sanitization().UseSubscriptionPathMaximumLength(30);

            Assert.AreEqual(30, validationSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionPathMaximumLength));
        }
    }
}