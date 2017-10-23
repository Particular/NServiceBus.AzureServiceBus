namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using AzureServiceBus;
    using Transport.AzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]

    public class When_using_default_configuration
    {
        [Test]
        public void Should_set_DeliveryCount_for_queues_to_10_attempts_for_no_immediate_retries_configured()
        {
            var settings = SettingsHolderFactory.BuildWithSerializer();

            DefaultConfigurationValues.Apply(settings);

            Assert.That(settings.Get<TopologySettings>().QueueSettings.MaxDeliveryCount, Is.EqualTo(10));
        }

        [Test]
        public void Should_set_DeliveryCount_for_subscriptions_to_10_attempts_for_no_immediate_retries_configured()
        {
            var settings = SettingsHolderFactory.BuildWithSerializer();

            DefaultConfigurationValues.Apply(settings);

            Assert.That(settings.Get<TopologySettings>().QueueSettings.MaxDeliveryCount, Is.EqualTo(10));
        }
    }
}
