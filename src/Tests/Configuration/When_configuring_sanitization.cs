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
    public class When_configuring_sanitization
    {
        [Test]
        public void Should_be_able_to_set_the_sanitization_strategy()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var topicSettings = extensions.Sanitization().UseStrategy<MySanitizationStrategy>();

            Assert.AreEqual(typeof(MySanitizationStrategy), topicSettings.GetSettings().Get<Type>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy));
        }

        class MySanitizationStrategy : ISanitizationStrategy
        {
            public string Sanitize(string entityPathOrName, EntityType entityType, ValidationResult validationResult)
            {
                throw new NotImplementedException();//not relevant for test
            }
        }
    }
}