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
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var idlePeriod = TimeSpan.FromDays(10);
            var topicSettings = extensions.UseDefaultTopology().Resources().Subscriptions().AutoDeleteOnIdle(idlePeriod);

            Assert.AreEqual(idlePeriod, topicSettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.AutoDeleteOnIdle));
        }

        [Test]
        public void Should_be_able_to_set_DefaultMessageTimeToLive()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            var expiryTimespan = TimeSpan.FromDays(1);
            var subscriptionSettings = extensions.UseDefaultTopology().Resources().Subscriptions().DefaultMessageTimeToLive(expiryTimespan);

            Assert.AreEqual(expiryTimespan, subscriptionSettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.DefaultMessageTimeToLive));
        }

        [Test]
        public void Should_be_able_to_set_EnableBatchedOperations()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            var subscriptionSettings = extensions.UseDefaultTopology().Resources().Subscriptions().EnableBatchedOperations(true);

            Assert.IsTrue(subscriptionSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableBatchedOperations));
        }

        [Test]
        public void Should_be_able_to_set_EnableDeadLetteringOnFilterEvaluationExceptions()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            var subscriptionSettings = extensions.UseDefaultTopology().Resources().Subscriptions().EnableDeadLetteringOnFilterEvaluationExceptions(true);

            Assert.IsTrue(subscriptionSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableDeadLetteringOnFilterEvaluationExceptions));
        }

        [Test]
        public void Should_be_able_to_set_EnableDeadLetteringOnMessageExpiration()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            var subscriptionSettings = extensions.UseDefaultTopology().Resources().Subscriptions().EnableDeadLetteringOnMessageExpiration(true);

            Assert.IsTrue(subscriptionSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableDeadLetteringOnMessageExpiration));
        }

        [Test]
        public void Should_be_able_to_set_ForwardDeadLetteredMessagesTo()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            var subscriptionSettings = extensions.UseDefaultTopology().Resources().Subscriptions().ForwardDeadLetteredMessagesTo("deadletteredmessages");

            Assert.AreEqual("deadletteredmessages", subscriptionSettings.GetSettings().Get<string>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesTo));
        }

        [Test]
        public void Should_be_able_to_set_ForwardDeadLetteredMessagesTo_conditionally()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            Func<string, bool> condition = n => n != "deadletteredmessages";
            var subscriptionSettings = extensions.UseDefaultTopology().Resources().Subscriptions().ForwardDeadLetteredMessagesTo(condition, "deadletteredmessages");

            Assert.AreEqual("deadletteredmessages", subscriptionSettings.GetSettings().Get<string>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesTo));
            Assert.AreEqual(condition, subscriptionSettings.GetSettings().Get<Func<string, bool>>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesToCondition));
        }

        [Test]
        public void Should_be_able_to_set_ForwardTo()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            var subscriptionSettings = extensions.UseDefaultTopology().Resources().Subscriptions().ForwardTo("forwardto");

            Assert.AreEqual("forwardto", subscriptionSettings.GetSettings().Get<string>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardTo));
        }

        [Test]
        public void Should_be_able_to_set_ForwardTo_conditionally()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            Func<string, bool> condition = n => n != "forwarded";
            var subscriptionSettings = extensions.UseDefaultTopology().Resources().Subscriptions().ForwardTo(condition, "forwarded");

            Assert.AreEqual("forwarded", subscriptionSettings.GetSettings().Get<string>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardTo));
            Assert.AreEqual(condition, subscriptionSettings.GetSettings().Get<Func<string, bool>>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardToCondition));
        }

        [Test]
        public void Should_be_able_to_set_LockDuration()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            var lockDuration = TimeSpan.FromDays(1);
            var subscriptionSettings = extensions.UseDefaultTopology().Resources().Subscriptions().LockDuration(lockDuration);

            Assert.AreEqual(lockDuration, subscriptionSettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.LockDuration));
        }

        [Test]
        public void Should_be_able_to_set_MaxDeliveryCount()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            const int selectedMaxDeliveryCount = 6;
            var subscriptionSettings = extensions.UseDefaultTopology().Resources().Subscriptions().MaxDeliveryCount(selectedMaxDeliveryCount);

            Assert.AreEqual(selectedMaxDeliveryCount, subscriptionSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.MaxDeliveryCount));
        }

        [Test]
        public void Should_be_able_to_set_RequiresSession()
        {
            var setting = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(setting);

            var subscriptionSettings = extensions.UseDefaultTopology().Resources().Subscriptions().RequiresSession(true);

            Assert.AreEqual(true, subscriptionSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.RequiresSession));
        }
    }
}