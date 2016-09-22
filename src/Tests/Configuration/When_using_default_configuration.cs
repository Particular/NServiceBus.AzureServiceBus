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
        public void Should_set_DeliveryCount_to_number_of_immediate_retries_plus_1_for_non_system_queues()
        {
            const int numberOfImmediateRetries = 3;

            var settings = new SettingsHolder();
            settings.Set(WellKnownConfigurationKeys.Core.RecoverabilityNumberOfImmediateRetries, numberOfImmediateRetries);
            new DefaultConfigurationValues().Apply(settings);

            var maxDeliveryCount = settings.Get<int>(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxDeliveryCount);

            Assert.That(maxDeliveryCount, Is.EqualTo(numberOfImmediateRetries + 1));
        }

        [Test]
        public void Should_set_DeliveryCount_for_queues_to_10_attempts_for_no_immediate_retries_configured()
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            Assert.That(settingsHolder.Get<int>(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxDeliveryCount), Is.EqualTo(10));
        }

        [Test]
        public void Should_set_DeliveryCount_for_subscriptions_to_10_attempts_for_no_immediate_retries_configured()
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            Assert.That(settingsHolder.Get<int>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.MaxDeliveryCount), Is.EqualTo(10));
        }
    }
}