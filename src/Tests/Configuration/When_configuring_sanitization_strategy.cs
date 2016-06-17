namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_sanitization_strategy
    {

        [Test]
        public void Should_be_able_to_set_the_sanitization_strategy()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            var strategy = extensions.Sanitization().UseStrategy<MySanitizationStrategy>();

            Assert.AreEqual(typeof(MySanitizationStrategy), strategy.GetSettings().Get<Type>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy));
        }

        [Test]
        public void Should_be_able_to_set_hash()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, string> hash = pathOrName => pathOrName;
            extensions.Sanitization().UseStrategy<MySanitizationStrategy>().Hash(hash);

            Assert.That(hash, Is.EqualTo(settings.Get<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Hash)));
        }


        class MySanitizationStrategy : ISanitizationStrategy
        {
            public string Sanitize(string entityPathOrName, EntityType entityType)
            {
                throw new NotImplementedException();//not relevant for test
            }
        }
    }
}