namespace NServiceBus.AzureServiceBus.Tests
{
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;
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

            var connectivitySettings = extensions.Connectivity().MessageReceivers().ReceiveMode(ReceiveMode.ReceiveAndDelete);

            Assert.AreEqual(ReceiveMode.ReceiveAndDelete, connectivitySettings.GetSettings().Get<ReceiveMode>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode));
        }

        [Test]
        public void Should_be_able_to_set_prefetchcount()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.Connectivity().MessageReceivers().PrefetchCount(1000);

            Assert.AreEqual(1000, connectivitySettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount));
        }

        [Test]
        public void Should_be_able_to_set_retrypolicy()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.Connectivity().MessageReceivers().RetryPolicy(RetryPolicy.NoRetry);

            Assert.IsInstanceOf<NoRetry>(connectivitySettings.GetSettings().Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.RetryPolicy));
        }
    }
}