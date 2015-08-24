namespace NServiceBus.AzureServiceBus.Tests
{
    using Microsoft.ServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_messaging_factories
    {
        [Test]
        public void Should_be_able_to_set_prefetchcount()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.Connectivity().MessagingFactories().PrefetchCount(1000);

            Assert.AreEqual(1000, connectivitySettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.PrefetchCount));
        }

        [Test]
        public void Should_be_able_to_set_retrypolicy()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.Connectivity().MessagingFactories().RetryPolicy(RetryPolicy.NoRetry);

            Assert.IsInstanceOf<NoRetry>(connectivitySettings.GetSettings().Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.RetryPolicy));
        }

    }
}