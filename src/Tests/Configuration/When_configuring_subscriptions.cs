namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_subscriptions
    {
        [Test]
        public void Should_be_able_to_set_AutoDeleteOnIdle()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);

            var idlePeriod = TimeSpan.FromDays(10);
            var topicSettings = extensions.Resources().Subscriptions().AutoDeleteOnIdle(idlePeriod);

            Assert.AreEqual(idlePeriod, topicSettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.AutoDeleteOnIdle));
        }

        [Test]
        public void Should_be_able_to_set_DefaultMessageTimeToLive()
        {
            var setting = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(setting);

            var expiryTimespan = TimeSpan.FromDays(1);
            var subscriptionSettings = extensions.Resources().Subscriptions().DefaultMessageTimeToLive(expiryTimespan);

            Assert.AreEqual(expiryTimespan, subscriptionSettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.DefaultMessageTimeToLive));
        }

        [Test]
        public void Should_be_able_to_set_EnableBatchedOperations()
        {
            var setting = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(setting);

            var subscriptionSettings = extensions.Resources().Subscriptions().EnableBatchedOperations(true);

            Assert.IsTrue(subscriptionSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableBatchedOperations));
        }

        [Test]
        public void Should_be_able_to_set_EnableDeadLetteringOnFilterEvaluationExceptions()
        {
            var setting = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(setting);

            var subscriptionSettings = extensions.Resources().Subscriptions().EnableDeadLetteringOnFilterEvaluationExceptions(true);

            Assert.IsTrue(subscriptionSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableDeadLetteringOnFilterEvaluationExceptions));
        }

        [Test]
        public void Should_be_able_to_set_EnableDeadLetteringOnMessageExpiration()
        {
            var setting = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(setting);

            var subscriptionSettings = extensions.Resources().Subscriptions().EnableDeadLetteringOnMessageExpiration(true);

            Assert.IsTrue(subscriptionSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableDeadLetteringOnMessageExpiration));
        }

        [Test]
        public void Should_be_able_to_set_ForwardDeadLetteredMessagesTo()
        {
            var setting = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(setting);

            var subscriptionSettings = extensions.Resources().Subscriptions().ForwardDeadLetteredMessagesTo("deadletteredmessages");

            Assert.AreEqual("deadletteredmessages", subscriptionSettings.GetSettings().Get<string>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesTo));
        }

        [Test]
        public void Should_be_able_to_set_ForwardDeadLetteredMessagesTo_conditionally()
        {
            var setting = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(setting);

            Func<string, bool> condition = n => n != "deadletteredmessages";
            var subscriptionSettings = extensions.Resources().Subscriptions().ForwardDeadLetteredMessagesTo(condition, "deadletteredmessages");

            Assert.AreEqual("deadletteredmessages", subscriptionSettings.GetSettings().Get<string>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesTo));
            Assert.AreEqual(condition, subscriptionSettings.GetSettings().Get<Func<string, bool>>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesToCondition));
        }

        [Test]
        public void Should_be_able_to_set_LockDuration()
        {
            var setting = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(setting);

            var lockDuration = TimeSpan.FromDays(1);
            var subscriptionSettings = extensions.Resources().Subscriptions().LockDuration(lockDuration);

            Assert.AreEqual(lockDuration, subscriptionSettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.LockDuration));
        }

        [Test]
        public void Should_be_able_to_set_MaxDeliveryCount()
        {
            var setting = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(setting);

            const int selectedMaxDeliveryCount = 6;
            var subscriptionSettings = extensions.Resources().Subscriptions().MaxDeliveryCount(selectedMaxDeliveryCount);

            Assert.AreEqual(selectedMaxDeliveryCount, subscriptionSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.MaxDeliveryCount));
        }

        [Test]
        public void Should_be_able_to_set_RequiresSession()
        {
            var setting = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(setting);

            var subscriptionSettings = extensions.Resources().Subscriptions().RequiresSession(true);

            Assert.AreEqual(true, subscriptionSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.RequiresSession));
        }
    }
}