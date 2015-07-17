namespace NServiceBus.AzureServiceBus.Tests
{
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_subscriptions
    {
        [Test]
        public void Should_be_able_to_set_support_ordering()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var subscriptionSettings = extensions.Topology().Resources().Subscriptions().SupportOrdering(true);

            Assert.IsTrue(subscriptionSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.SupportOrdering));
        }
    }
}