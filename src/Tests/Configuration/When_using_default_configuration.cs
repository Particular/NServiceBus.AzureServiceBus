namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using AzureServiceBus;
    using Transport.AzureServiceBus;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    [Category("AzureServiceBus")]

    public class When_using_default_configuration
    {
        [Test]
        public void Should_set_DeliveryCount_for_queues_to_10_attempts_for_no_immediate_retries_configured()
        {
            var settings = SettingsHolderFactory.BuildWithSerializer();
            settings.Set<TopologySettings>(new TopologySettings());

            DefaultConfigurationValues.Apply(settings);

            Assert.That(settings.Get<TopologySettings>().QueueSettings.MaxDeliveryCount, Is.EqualTo(10));
        }

        [Test]
        public void Should_set_DeliveryCount_for_subscriptions_to_10_attempts_for_no_immediate_retries_configured()
        {
            var settings = SettingsHolderFactory.BuildWithSerializer();
            settings.Set<TopologySettings>(new TopologySettings());

            DefaultConfigurationValues.Apply(settings);

            Assert.That(settings.Get<TopologySettings>().QueueSettings.MaxDeliveryCount, Is.EqualTo(10));
        }

        [Test]
        public void Should_throw_exception_when_no_serializer_was_set()
        {
            var settings = new SettingsHolder();
            settings.Set<TopologySettings>(new TopologySettings());

            var exception = Assert.Throws<Exception>(() => DefaultConfigurationValues.Apply(settings));

            Assert.IsTrue(exception.Message.StartsWith("Use 'endpointConfiguration.UseSerialization<T>();'"), $"Incorrect exception message: {exception.Message}");
        }
    }
}
