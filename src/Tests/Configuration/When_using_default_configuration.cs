namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
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
            var settingsHolder = new SettingsHolder();
            settingsHolder.Set<TopologySettings>(new TopologySettings());
            new DefaultConfigurationValues().Apply(settingsHolder);

            Assert.That(settingsHolder.Get<TopologySettings>().QueueSettings.MaxDeliveryCount, Is.EqualTo(10));
        }

        [Test]
        public void Should_set_DeliveryCount_for_subscriptions_to_10_attempts_for_no_immediate_retries_configured()
        {
            var settingsHolder = new SettingsHolder();
            settingsHolder.Set<TopologySettings>(new TopologySettings());
            new DefaultConfigurationValues().Apply(settingsHolder);

            Assert.That(settingsHolder.Get<TopologySettings>().QueueSettings.MaxDeliveryCount, Is.EqualTo(10));
        }
    }
}