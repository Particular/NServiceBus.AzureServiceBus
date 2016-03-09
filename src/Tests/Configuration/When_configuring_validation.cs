namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_validation
    {
        private SettingsHolder settingsHolder;
        private AzureServiceBusTopologySettings extensions;

        [SetUp]
        public void SetUp()
        {
            settingsHolder = new SettingsHolder();
            extensions = new AzureServiceBusTopologySettings(settingsHolder);
        }
        
        [Test]
        public void Should_be_able_to_set_the_validation_strategy()
        {
            var validationSettings = extensions.Addressing().Validation().UseStrategy<MyValidationStrategy>();

            Assert.AreEqual(typeof(MyValidationStrategy), validationSettings.GetSettings().Get<Type>(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy));
        }

        class MyValidationStrategy : IValidationStrategy
        {
            public bool IsValid(string entityPath, EntityType entityType)
            {
                throw new NotImplementedException(); // not relevant for test
            }
        }


        [Test]
        public void Should_be_able_to_set_queue_path_maximum_length()
        {
            var validationSettings = extensions.Addressing().Validation().UseQueuePathMaximumLength(10);

            Assert.AreEqual(10, validationSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.QueuePathMaximumLength));
        }

        [Test]
        public void Should_be_able_to_set_topic_path_maximum_length()
        {
            var validationSettings = extensions.Addressing().Validation().UseTopicPathMaximumLength(20);

            Assert.AreEqual(20, validationSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.TopicPathMaximumLength));
        }

        [Test]
        public void Should_be_able_to_set_subscription_path_maximum_length()
        {
            var validationSettings = extensions.Addressing().Validation().UseSubscriptionPathMaximumLength(30);

            Assert.AreEqual(30, validationSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.SubscriptionPathMaximumLength));
        }
    }
}