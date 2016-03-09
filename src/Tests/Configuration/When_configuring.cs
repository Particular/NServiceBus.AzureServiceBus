namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using FakeItEasy;
    using NServiceBus.AzureServiceBus;
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
            var extensions = new AzureServiceBusTopologySettings(settings);

            var topologySettings = extensions.UseTopology(A.Fake<ITopology>);

            Assert.IsInstanceOf<AzureServiceBusTopologySettings>(topologySettings);
        }

        [Test]
        public void Should_be_able_to_extend_addressing_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);

            var addressingSettings = extensions.Addressing();

            Assert.IsInstanceOf<AzureServiceBusAddressingSettings>(addressingSettings);
        }

        [Test]
        public void Should_be_able_to_extend_namespace_partitioning_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);

            var partitioningSettings = extensions.Addressing().NamespacePartitioning();

            Assert.IsInstanceOf<AzureServiceBusNamespacePartitioningSettings>(partitioningSettings);
        }

        [Test]
        public void Should_be_able_to_extend_composition_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);

            var compositionSettings = extensions.Addressing().Composition();

            Assert.IsInstanceOf<AzureServiceBusCompositionSettings>(compositionSettings);
        }

        [Test]
        public void Should_be_able_to_extend_validation_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);

            var validationSettings = extensions.Addressing().Validation();

            Assert.IsInstanceOf<AzureServiceBusValidationSettings>(validationSettings);
        }

        [Test]
        public void Should_be_able_to_extend_individualization_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);

            var individualizationSettings = extensions.Addressing().Individualization();

            Assert.IsInstanceOf<AzureServiceBusIndividualizationSettings>(individualizationSettings);
        }

        [Test]
        public void Should_be_able_to_extend_resource_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);

            var resourceSettings = extensions.Resources();

            Assert.IsInstanceOf<AzureServiceBusResourceSettings>(resourceSettings);
        }

        [Test]
        public void Should_be_able_to_extend_queue_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);

            var queueSettings = extensions.Resources().Queues();

            Assert.IsInstanceOf<AzureServiceBusQueueSettings>(queueSettings);
        }

        [Test]
        public void Should_be_able_to_extend_topic_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);

            var topicsSettings = extensions.Resources().Topics();

            Assert.IsInstanceOf<AzureServiceBusTopicSettings>(topicsSettings);
        }

        [Test]
        public void Should_be_able_to_extend_subscription_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);

            var subscriptionSettings = extensions.Resources().Subscriptions();

            Assert.IsInstanceOf<AzureServiceBusSubscriptionSettings>(subscriptionSettings);
        }

        [Test]
        public void Should_be_able_to_extend_transaction_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);

            var transactionSettings = extensions.Transactions();

            Assert.IsInstanceOf<AzureServiceBusTransactionSettings>(transactionSettings);
        }

        [Test]
        public void Should_be_able_to_extend_connectivity_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);

            var connectivitySettings = extensions.Connectivity();

            Assert.IsInstanceOf<AzureServiceBusConnectivitySettings>(connectivitySettings);
        }

        [Test]
        public void Should_be_able_to_extend_serialization_settings()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);

            var connectivitySettings = extensions.Serialization();

            Assert.IsInstanceOf<AzureServiceBusSerializationSettings>(connectivitySettings);
        }
    }
}