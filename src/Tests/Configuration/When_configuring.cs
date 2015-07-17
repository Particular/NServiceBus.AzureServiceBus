namespace NServiceBus.AzureServiceBus.Tests
{
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring
    {
        [Test]
        public void Should_be_able_to_extend_topology_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var topologySettings = extensions.Topology();

            Assert.IsInstanceOf<AzureServiceBusTopologySettings>(topologySettings);
        }

        [Test]
        public void Should_be_able_to_extend_addressing_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var addressingSettings = extensions.Topology().Addressing();

            Assert.IsInstanceOf<AzureServiceBusAddressingSettings>(addressingSettings);
        }

        [Test]
        public void Should_be_able_to_extend_resource_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var resourceSettings = extensions.Topology().Resources();

            Assert.IsInstanceOf<AzureServiceBusResourceSettings>(resourceSettings);
        }

        [Test]
        public void Should_be_able_to_extend_queue_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var resourceSettings = extensions.Topology().Resources().Queues();

            Assert.IsInstanceOf<AzureServiceBusQueueSettings>(resourceSettings);
        }
    }
}