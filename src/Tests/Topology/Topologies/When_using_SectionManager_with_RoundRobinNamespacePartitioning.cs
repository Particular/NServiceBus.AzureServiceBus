namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.Topologies
{
    using System.Linq;
    using AzureServiceBus.Topology.MetaModel;
    using NUnit.Framework;
    using Settings;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_SectionManager_with_RoundRobinNamespacePartitioning
    {
        static string PrimaryConnectionString = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        static string SecondaryConnectionString = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        static string PrimaryName = "namespace1";
        static string SecondaryName = "namespace2";

        RoundRobinNamespacePartitioning namespacePartitioningStrategy;
        SettingsHolder settings;

        [SetUp]
        public void SetUp()
        {
            settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.NamespacePartitioning().AddNamespace(PrimaryName, PrimaryConnectionString);
            extensions.NamespacePartitioning().AddNamespace(SecondaryName, SecondaryConnectionString);

            namespacePartitioningStrategy = new RoundRobinNamespacePartitioning(settings);

            // apply entity maximum lengths for addressing logic
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathMaximumLength, 260);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathMaximumLength, 260);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameMaximumLength, 50);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameMaximumLength, 50);
        }

        [Test]
        public void Should_alternate_between_namespaces_for_ForwardingTopologySectionManager_for_publishing()
        {
            var namespaceConfigurations = new NamespaceConfigurations();
            var addressingLogic = new AddressingLogic(new ThrowOnFailedValidation(settings), new FlatComposition());
            var sectionManager = new ForwardingTopologySectionManager(PrimaryName, namespaceConfigurations, "sales", 1, "bundle", namespacePartitioningStrategy, addressingLogic, new DefaultCreateBrokerSideSubscriptionFilter());
            sectionManager.BundleConfigurations = new NamespaceBundleConfigurations
            {
                {PrimaryName, 1},
                {SecondaryName, 1}
            };

            sectionManager.DeterminePublishDestination(typeof(SomeEvent), "sales");
            var publishDestination2 = sectionManager.DeterminePublishDestination(typeof(SomeEvent), "sales");
            Assert.AreEqual(SecondaryName, publishDestination2.Entities.First().Namespace.Alias, "Should have different namespace");
        }

        [Test]
        public void Should_alternate_between_namespaces_for_EndpointOrientedTopologySectionManager_for_publishing()
        {
            var namespaceConfigurations = new NamespaceConfigurations();
            var addressingLogic = new AddressingLogic(new ThrowOnFailedValidation(settings), new FlatComposition());
            var conventions = new Conventions();
            conventions.AddSystemMessagesConventions(type => type != typeof(SomeEvent));
            var publishersConfiguration = new PublishersConfiguration(conventions, new SettingsHolder());
            var sectionManager = new EndpointOrientedTopologySectionManager(PrimaryName, namespaceConfigurations, "sales", publishersConfiguration, namespacePartitioningStrategy, addressingLogic, new DefaultCreateBrokerSideSubscriptionFilter());

            sectionManager.DeterminePublishDestination(typeof(SomeEvent), "sales");
            var publishDestination2 = sectionManager.DeterminePublishDestination(typeof(SomeEvent), "sales");
            Assert.AreEqual(SecondaryName, publishDestination2.Entities.First().Namespace.Alias, "Should have different namespace");
        }

        class SomeEvent : IEvent
        {
        }

        [Test]
        public void Should_alternate_between_namespaces_for_ForwardingTopologySectionManager_for_sending()
        {
            var namespaceConfigurations = new NamespaceConfigurations();
            var addressingLogic = new AddressingLogic(new ThrowOnFailedValidation(settings), new FlatComposition());
            var sectionManager = new ForwardingTopologySectionManager(PrimaryName, namespaceConfigurations, "sales", 1, "bundle", namespacePartitioningStrategy, addressingLogic, new DefaultCreateBrokerSideSubscriptionFilter());
            sectionManager.BundleConfigurations = new NamespaceBundleConfigurations
            {
                {PrimaryName, 1},
                {SecondaryName, 1}
            };

            sectionManager.DetermineSendDestination($"sales@{PrimaryName}");
            var publishDestination2 = sectionManager.DetermineSendDestination($"sales@{PrimaryName}");
            Assert.AreEqual(SecondaryName, publishDestination2.Entities.First().Namespace.Alias, "Should have different namespace");
        }

        [Test]
        public void Should_alternate_between_namespaces_for_EndpointOrientedTopologySectionManager_for_sending()
        {
            var namespaceConfigurations = new NamespaceConfigurations();
            var addressingLogic = new AddressingLogic(new ThrowOnFailedValidation(settings), new FlatComposition());
            var conventions = new Conventions();
            conventions.AddSystemMessagesConventions(type => type != typeof(SomeEvent));
            var publishersConfiguration = new PublishersConfiguration(conventions, new SettingsHolder());
            var sectionManager = new EndpointOrientedTopologySectionManager(PrimaryName, namespaceConfigurations, "sales", publishersConfiguration, namespacePartitioningStrategy, addressingLogic, new DefaultCreateBrokerSideSubscriptionFilter());

            sectionManager.DetermineSendDestination($"sales@{PrimaryName}");
            var sendDestination2 = sectionManager.DetermineSendDestination($"sales@{PrimaryName}");
            Assert.AreEqual(SecondaryName, sendDestination2.Entities.First().Namespace.Alias, "Should have different namespace");
        }
    }
}