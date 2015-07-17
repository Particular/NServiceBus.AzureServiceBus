namespace NServiceBus.AzureServiceBus.Tests
{
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_queues
    {
        [Test]
        public void Should_be_able_to_set_support_ordering()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var queueSettings = extensions.Topology().Resources().Queues().SupportOrdering(true);

            Assert.IsTrue(queueSettings.GetSettings().Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.SupportOrdering));
        }
    }
}