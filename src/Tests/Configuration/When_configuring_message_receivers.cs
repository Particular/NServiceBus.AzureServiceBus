namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Transport.AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_message_receivers
    {
        [Test]
        public void Should_be_able_to_set_receivemode()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.MessageReceivers().ReceiveMode(ReceiveMode.ReceiveAndDelete);

            Assert.AreEqual(ReceiveMode.ReceiveAndDelete, connectivitySettings.GetSettings().Get<ReceiveMode>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode));
        }

        [Test]
        public void Should_be_able_to_set_prefetchcount()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.MessageReceivers().PrefetchCount(1000);

            Assert.AreEqual(1000, connectivitySettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount));
        }

        [Test]
        public void Should_be_able_to_set_retrypolicy()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.MessageReceivers().RetryPolicy(RetryPolicy.NoRetry);

            Assert.IsInstanceOf<NoRetry>(connectivitySettings.GetSettings().Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.RetryPolicy));
        }

        [Test]
        public void Should_be_able_to_set_autorenewtimeout()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.MessageReceivers().AutoRenewTimeout(TimeSpan.FromSeconds(60));

            Assert.AreEqual(TimeSpan.FromSeconds(60), connectivitySettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.AutoRenewTimeout));
        }
    }
}