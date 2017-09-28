namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using Settings;
    using NUnit.Framework;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring
    {
        [Test]
        public void Should_be_able_to_extend_topology_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var topologySettings = extensions.UseForwardingTopology();

            Assert.IsInstanceOf<TransportExtensions<AzureServiceBusTransport>>(topologySettings);
        }

        [Test]
        public void Should_be_able_to_extend_namespace_partitioning_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var partitioningSettings = extensions.NamespacePartitioning();

            Assert.IsInstanceOf<AzureServiceBusNamespacePartitioningSettings>(partitioningSettings);
        }

        [Test]
        public void Should_be_able_to_extend_composition_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var compositionSettings = extensions.Composition();

            Assert.IsInstanceOf<AzureServiceBusCompositionSettings>(compositionSettings);
        }

        [Test]
        public void Should_be_able_to_extend_individualization_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var individualizationSettings = extensions.Individualization();

            Assert.IsInstanceOf<AzureServiceBusIndividualizationSettings>(individualizationSettings);
        }

        [Test]
        public void Should_be_able_to_extend_queue_settings()
        {
            var settings = new SettingsHolder();
            settings.Set<TopologySettings>(new TopologySettings());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var queueSettings = extensions.Queues();

            Assert.IsInstanceOf<AzureServiceBusQueueSettings>(queueSettings);
        }

        [Test]
        public void Should_be_able_to_extend_topic_settings()
        {
            var settings = new SettingsHolder();
            settings.Set<TopologySettings>(new TopologySettings());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var topicsSettings = extensions.Topics();

            Assert.IsInstanceOf<AzureServiceBusTopicSettings>(topicsSettings);
        }

        [Test]
        public void Should_be_able_to_extend_subscription_settings()
        {
            var settings = new SettingsHolder();
            settings.Set<TopologySettings>(new TopologySettings());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var subscriptionSettings = extensions.Subscriptions();

            Assert.IsInstanceOf<AzureServiceBusSubscriptionSettings>(subscriptionSettings);
        }
    }
}