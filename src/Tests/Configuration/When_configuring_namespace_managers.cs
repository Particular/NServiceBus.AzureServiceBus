namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using Microsoft.ServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NUnit.Framework;
    using Settings;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_namespace_managers
    {
        [Test]
        public void Should_be_able_to_set_namespace_managers_settings_factory_method()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, NamespaceManagerSettings> registeredFactory = s => new NamespaceManagerSettings();

            var connectivitySettings = extensions.NamespaceManagers().NamespaceManagerSettingsFactory(registeredFactory);

            Assert.AreEqual(registeredFactory, connectivitySettings.GetSettings().Get<Func<string, NamespaceManagerSettings>>(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.NamespaceManagerSettingsFactory));
        }

        [Test]
        public void Should_be_able_to_set_retrypolicy()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.NamespaceManagers().RetryPolicy(RetryPolicy.NoRetry);

            Assert.IsInstanceOf<NoRetry>(connectivitySettings.GetSettings().Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.RetryPolicy));
        }

        [Test]
        public void Should_be_able_to_set_token_provider_factory_method()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, TokenProvider> registeredFactory = s => null;//illegal token provider, but don't want to provide credential info

            var connectivitySettings = extensions.NamespaceManagers().TokenProvider(registeredFactory);

            Assert.AreEqual(registeredFactory, connectivitySettings.GetSettings().Get<Func<string, TokenProvider>>(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.TokenProviderFactory));
        }

    }
}