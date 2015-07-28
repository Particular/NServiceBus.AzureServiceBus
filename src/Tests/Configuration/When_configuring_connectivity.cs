namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_connectivity
    {
        [Test]
        public void Should_be_able_to_set_messaging_factory_settings_factory_method()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, MessagingFactorySettings> registeredFactory = s => new MessagingFactorySettings();

            var connectivitySettings = extensions.Connectivity().MessagingFactorySettings(registeredFactory);

            Assert.AreEqual(registeredFactory, connectivitySettings.GetSettings().Get<Func<string, MessagingFactorySettings>>(WellKnownConfigurationKeys.Connectivity.MessagingFactorySettingsFactory));
        }

        [Test]
        public void Should_be_able_to_set_number_of_messaging_factories_per_namespace()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.Connectivity().NumberOfMessagingFactoriesPerNamespace(4);

            Assert.AreEqual(4, connectivitySettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfMessagingFactoriesPerNamespace));
        }

        [Test]
        public void Should_be_able_to_set_number_of_message_receivers_per_entity()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.Connectivity().NumberOfMessageReceiversPerEntity(4);

            Assert.AreEqual(4, connectivitySettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfMessageReceiversPerEntity));
        }

    }

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_resource_creation
    {
        [Test]
        public void Should_be_able_to_set_queue_description_factory_method()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, QueueDescription> registeredFactory = s => new QueueDescription("myqueue");

            var connectivitySettings = extensions.Topology().Resources().QueueDescriptions(registeredFactory);

            Assert.AreEqual(registeredFactory, connectivitySettings.GetSettings().Get<Func<string, QueueDescription>>(WellKnownConfigurationKeys.Topology.Resources.QueueDescriptionsFactory));
        }

        [Test]
        public void Should_be_able_to_set_topic_description_factory_method()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, TopicDescription> registeredFactory = s => new TopicDescription("mytopic");

            var connectivitySettings = extensions.Topology().Resources().TopicDescriptions(registeredFactory);

            Assert.AreEqual(registeredFactory, connectivitySettings.GetSettings().Get<Func<string, TopicDescription>>(WellKnownConfigurationKeys.Topology.Resources.TopicDescriptionsFactory));
        }

        [Test]
        public void Should_be_able_to_set_subscription_description_factory_method()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, SubscriptionDescription> registeredFactory = s => new SubscriptionDescription("mytopic", "mysubscription");

            var connectivitySettings = extensions.Topology().Resources().SubscriptionDescriptions(registeredFactory);

            Assert.AreEqual(registeredFactory, connectivitySettings.GetSettings().Get<Func<string, SubscriptionDescription>>(WellKnownConfigurationKeys.Topology.Resources.SubscriptionDescriptionsFactory));
        }

    }
}