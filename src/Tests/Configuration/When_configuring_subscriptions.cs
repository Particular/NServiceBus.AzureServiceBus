namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_subscriptions
    {
        [Test]
        public void Should_be_able_to_set_subscription_description_factory_method()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Action<SubscriptionDescription> registeredFactory = sd => { };

            extensions.Subscriptions().DescriptionFactory(registeredFactory);

            Assert.AreEqual(registeredFactory, topology.Settings.SubscriptionSettings.DescriptionCustomizer);
        }

        [Test]
        public void Should_be_able_to_set_AutoDeleteOnIdle()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var idlePeriod = TimeSpan.FromDays(10);
            extensions.Subscriptions().AutoDeleteOnIdle(idlePeriod);

            Assert.AreEqual(idlePeriod, topology.Settings.SubscriptionSettings.AutoDeleteOnIdle);
        }

        [Test]
        public void Should_be_able_to_set_DefaultMessageTimeToLive()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var expiryTimespan = TimeSpan.FromDays(1);
            extensions.Subscriptions().DefaultMessageTimeToLive(expiryTimespan);

            Assert.AreEqual(expiryTimespan, topology.Settings.SubscriptionSettings.DefaultMessageTimeToLive);
        }

        [Test]
        public void Should_be_able_to_set_EnableBatchedOperations()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Subscriptions().EnableBatchedOperations(true);

            Assert.IsTrue(topology.Settings.SubscriptionSettings.EnableBatchedOperations);
        }

        [Test]
        public void Should_be_able_to_set_EnableDeadLetteringOnFilterEvaluationExceptions()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Subscriptions().EnableDeadLetteringOnFilterEvaluationExceptions(true);

            Assert.IsTrue(topology.Settings.SubscriptionSettings.EnableDeadLetteringOnFilterEvaluationExceptions);
        }

        [Test]
        public void Should_be_able_to_set_EnableDeadLetteringOnMessageExpiration()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

           extensions.Subscriptions().EnableDeadLetteringOnMessageExpiration(true);

            Assert.IsTrue(topology.Settings.SubscriptionSettings.EnableDeadLetteringOnMessageExpiration);
        }

        [Test]
        public void Should_be_able_to_set_ForwardDeadLetteredMessagesTo()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Subscriptions().ForwardDeadLetteredMessagesTo("deadletteredmessages");

            Assert.AreEqual("deadletteredmessages", topology.Settings.SubscriptionSettings.ForwardDeadLetteredMessagesTo);
        }

        [Test]
        public void Should_be_able_to_set_ForwardDeadLetteredMessagesTo_conditionally()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, bool> condition = n => n != "deadletteredmessages";
            extensions.Subscriptions().ForwardDeadLetteredMessagesTo(condition, "deadletteredmessages");

            Assert.AreEqual("deadletteredmessages", topology.Settings.SubscriptionSettings.ForwardDeadLetteredMessagesTo);
            Assert.AreEqual(condition, topology.Settings.SubscriptionSettings.ForwardDeadLetteredMessagesToCondition);
        }

        [Test]
        public void Should_be_able_to_set_LockDuration()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var lockDuration = TimeSpan.FromDays(1);
            extensions.Subscriptions().LockDuration(lockDuration);

            Assert.AreEqual(lockDuration, topology.Settings.SubscriptionSettings.LockDuration);
        }

        [Test]
        public void Should_be_able_to_set_MaxDeliveryCount()
        {
            var settings = new SettingsHolder();
            var topology = new FakeTopology(settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            const int selectedMaxDeliveryCount = 6;
            extensions.Subscriptions().MaxDeliveryCount(selectedMaxDeliveryCount);

            Assert.AreEqual(selectedMaxDeliveryCount, topology.Settings.SubscriptionSettings.MaxDeliveryCount);
        }
    }
}