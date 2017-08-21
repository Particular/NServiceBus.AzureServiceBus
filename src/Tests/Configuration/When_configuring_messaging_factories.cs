namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using Microsoft.ServiceBus;
    using Transport.AzureServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Configuration.AdvancedExtensibility;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_messaging_factories
    {
        [Test]
        public void Should_be_able_to_set_messaging_factory_settings_factory_method()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, MessagingFactorySettings> registeredFactory = s => new MessagingFactorySettings();

            var connectivitySettings = extensions.MessagingFactories().MessagingFactorySettingsFactory(registeredFactory);

            Assert.AreEqual(registeredFactory, connectivitySettings.GetSettings().Get<Func<string, MessagingFactorySettings>>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.MessagingFactorySettingsFactory));
        }

        [Test]
        public void Should_be_able_to_set_number_of_messaging_factories_per_namespace()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.MessagingFactories().NumberOfMessagingFactoriesPerNamespace(4);

            Assert.AreEqual(4, connectivitySettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace));
        }

        [Test]
        public void Should_be_able_to_set_retrypolicy()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.MessagingFactories().RetryPolicy(RetryPolicy.NoRetry);

            Assert.IsInstanceOf<NoRetry>(connectivitySettings.GetSettings().Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.RetryPolicy));
        }

        [Test]
        public void Should_be_able_to_set_batchflushinterval()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.MessagingFactories().BatchFlushInterval(TimeSpan.FromSeconds(0));

            Assert.AreEqual(TimeSpan.FromSeconds(0), connectivitySettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.BatchFlushInterval));
        }

    }
}