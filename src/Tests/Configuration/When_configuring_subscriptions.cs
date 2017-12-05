namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using Settings;
    using NUnit.Framework;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_subscriptions
    {
        SettingsHolder settings;
        TransportExtensions<AzureServiceBusTransport> extensions;

        [SetUp]
        public void SetUp()
        {
            settings = new SettingsHolder();
            extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
        }

        [Test]
        public void Should_be_able_to_set_subscription_description_factory_method()
        {
            Action<SubscriptionDescription> registeredFactory = sd => { };

            extensions.Subscriptions().DescriptionCustomizer(registeredFactory);

            Assert.AreEqual(registeredFactory, settings.Get<TopologySettings>().SubscriptionSettings.DescriptionCustomizer);
        }

        [Test]
        public void Should_be_able_to_set_AutoDeleteOnIdle()
        {
            var idlePeriod = TimeSpan.FromDays(10);
            extensions.Subscriptions().AutoDeleteOnIdle(idlePeriod);

            Assert.AreEqual(idlePeriod, settings.Get<TopologySettings>().SubscriptionSettings.AutoDeleteOnIdle);
        }

        [Test]
        public void Should_be_able_to_set_DefaultMessageTimeToLive()
        {
            var expiryTimespan = TimeSpan.FromDays(1);
            extensions.Subscriptions().DefaultMessageTimeToLive(expiryTimespan);

            Assert.AreEqual(expiryTimespan, settings.Get<TopologySettings>().SubscriptionSettings.DefaultMessageTimeToLive);
        }

        [Test]
        public void Should_be_able_to_set_EnableBatchedOperations()
        {
            extensions.Subscriptions().EnableBatchedOperations(true);

            Assert.IsTrue(settings.Get<TopologySettings>().SubscriptionSettings.EnableBatchedOperations);
        }

        [Test]
        public void Should_be_able_to_set_EnableDeadLetteringOnFilterEvaluationExceptions()
        {
            extensions.Subscriptions().EnableDeadLetteringOnFilterEvaluationExceptions(true);

            Assert.IsTrue(settings.Get<TopologySettings>().SubscriptionSettings.EnableDeadLetteringOnFilterEvaluationExceptions);
        }

        [Test]
        public void Should_be_able_to_set_EnableDeadLetteringOnMessageExpiration()
        {
            extensions.Subscriptions().EnableDeadLetteringOnMessageExpiration(true);

            Assert.IsTrue(settings.Get<TopologySettings>().SubscriptionSettings.EnableDeadLetteringOnMessageExpiration);
        }

        [Test]
        public void Should_be_able_to_set_ForwardDeadLetteredMessagesTo()
        {
            extensions.Subscriptions().ForwardDeadLetteredMessagesTo("deadletteredmessages");

            Assert.AreEqual("deadletteredmessages", settings.Get<TopologySettings>().SubscriptionSettings.ForwardDeadLetteredMessagesTo);
        }

        [Test]
        public void Should_be_able_to_set_ForwardDeadLetteredMessagesTo_conditionally()
        {
            Func<string, bool> condition = n => n != "deadletteredmessages";
            extensions.Subscriptions().ForwardDeadLetteredMessagesTo(condition, "deadletteredmessages");

            Assert.AreEqual("deadletteredmessages", settings.Get<TopologySettings>().SubscriptionSettings.ForwardDeadLetteredMessagesTo);
            Assert.AreEqual(condition, settings.Get<TopologySettings>().SubscriptionSettings.ForwardDeadLetteredMessagesToCondition);
        }

        [Test]
        public void Should_be_able_to_set_LockDuration()
        {
            var lockDuration = TimeSpan.FromDays(1);
            extensions.Subscriptions().LockDuration(lockDuration);

            Assert.AreEqual(lockDuration, settings.Get<TopologySettings>().SubscriptionSettings.LockDuration);
        }
    }
}