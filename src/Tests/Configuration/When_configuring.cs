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

            var topologySettings = extensions.UseDefaultTopology();

            Assert.IsInstanceOf<AzureServiceBusTopologySettings>(topologySettings);
        }

        [Test]
        public void Should_be_able_to_extend_addressing_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var addressingSettings = extensions.UseDefaultTopology().Addressing();

            Assert.IsInstanceOf<AzureServiceBusAddressingSettings>(addressingSettings);
        }

        [Test]
        public void Should_be_able_to_extend_namespace_partitioning_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var partitioningSettings = extensions.UseDefaultTopology().Addressing().NamespacePartitioning();

            Assert.IsInstanceOf<AzureServiceBusNamespacePartitioningSettings>(partitioningSettings);
        }

        [Test]
        public void Should_be_able_to_extend_composition_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var compositionSettings = extensions.UseDefaultTopology().Addressing().Composition();

            Assert.IsInstanceOf<AzureServiceBusCompositionSettings>(compositionSettings);
        }

        [Test]
        public void Should_be_able_to_extend_validation_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var validationSettings = extensions.UseDefaultTopology().Addressing().Validation();

            Assert.IsInstanceOf<AzureServiceBusValidationSettings>(validationSettings);
        }

        [Test]
        public void Should_be_able_to_extend_individualization_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var individualizationSettings = extensions.UseDefaultTopology().Addressing().Individualization();

            Assert.IsInstanceOf<AzureServiceBusIndividualizationSettings>(individualizationSettings);
        }

        [Test]
        public void Should_be_able_to_extend_resource_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var resourceSettings = extensions.UseDefaultTopology().Resources();

            Assert.IsInstanceOf<AzureServiceBusResourceSettings>(resourceSettings);
        }

        [Test]
        public void Should_be_able_to_extend_queue_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var queueSettings = extensions.UseDefaultTopology().Resources().Queues();

            Assert.IsInstanceOf<AzureServiceBusQueueSettings>(queueSettings);
        }

        [Test]
        public void Should_be_able_to_extend_topic_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var topicsSettings = extensions.UseDefaultTopology().Resources().Topics();

            Assert.IsInstanceOf<AzureServiceBusTopicSettings>(topicsSettings);
        }

        [Test]
        public void Should_be_able_to_extend_subscription_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var subscriptionSettings = extensions.UseDefaultTopology().Resources().Subscriptions();

            Assert.IsInstanceOf<AzureServiceBusSubscriptionSettings>(subscriptionSettings);
        }

        [Test]
        public void Should_be_able_to_extend_batching_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var batchingSettings = extensions.Batching();

            Assert.IsInstanceOf<AzureServiceBusBatchingSettings>(batchingSettings);
        }

        [Test]
        public void Should_be_able_to_extend_transaction_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var transactionSettings = extensions.Transactions();

            Assert.IsInstanceOf<AzureServiceBusTransactionSettings>(transactionSettings);
        }

        [Test]
        public void Should_be_able_to_extend_connectivity_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.Connectivity();

            Assert.IsInstanceOf<AzureServiceBusConnectivitySettings>(connectivitySettings);
        }

        [Test]
        public void Should_be_able_to_extend_serialization_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.Serialization();

            Assert.IsInstanceOf<AzureServiceBusSerializationSettings>(connectivitySettings);
        }
    }
}