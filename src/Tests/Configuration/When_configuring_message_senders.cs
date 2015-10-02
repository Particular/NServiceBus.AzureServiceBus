namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using Microsoft.ServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_message_senders
    {
        [Test]
        public void Should_be_able_to_set_retrypolicy()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.Connectivity().MessageSenders().RetryPolicy(RetryPolicy.NoRetry);

            Assert.IsInstanceOf<NoRetry>(connectivitySettings.GetSettings().Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryPolicy));
        }

        [Test]
        public void Should_be_able_to_set_backoff_time_when_throttled()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.Connectivity().MessageSenders().BackOffTimeOnThrottle(TimeSpan.FromSeconds(20));

            Assert.AreEqual(TimeSpan.FromSeconds(20), connectivitySettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessageSenders.BackOffTimeOnThrottle));
        }

        [Test]
        public void Should_be_able_to_set_retry_attempts_when_throttled()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.Connectivity().MessageSenders().RetryAttemptsOnThrottle(10);

            Assert.AreEqual(10, connectivitySettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryAttemptsOnThrottle));
        }
    }
}