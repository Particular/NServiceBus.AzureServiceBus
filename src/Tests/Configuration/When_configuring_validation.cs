namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_validation
    {
        [Test]
        public void Should_be_able_to_set_the_validation_strategy()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var topicSettings = extensions.Topology().Addressing().Validation().Strategy<MyValidationStrategy>();

            Assert.AreEqual(typeof(MyValidationStrategy), topicSettings.GetSettings().Get<Type>(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy));
        }

        class MyValidationStrategy : IValidationStrategy
        {
        }
    }
}