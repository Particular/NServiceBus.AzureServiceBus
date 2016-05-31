namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using AzureServiceBus;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    [Category("AzureServiceBus")]

    public class When_using_default_configuration
    {
        [Test]
        public void Should_set_DeliveryCount_for_queues_to_10_attempts()
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            Assert.That(settingsHolder.Get<int>(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxDeliveryCount), Is.EqualTo(10));
        }

        [Test]
        public void Should_set_DeliveryCount_for_subscriptions_to_10_attempts()
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            Assert.That(settingsHolder.Get<int>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.MaxDeliveryCount), Is.EqualTo(10));
        }
    }
}