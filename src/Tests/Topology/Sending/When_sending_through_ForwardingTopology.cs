namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.Sending
{
    using System.Linq;
    using AzureServiceBus;
    using TestUtils;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sending_through_ForwardingTopology
    {
        [Test]
        public void Should_set_a_signle_queue_as_destination_for_command()
        {
            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = SetupForwardingTopology(container, "sales");

            var destination = topology.DetermineSendDestination("operations");

            Assert.IsTrue(destination.Entities.Single().Type == EntityType.Queue);
            Assert.IsTrue(destination.Entities.Single().Path == "operations");
        }

        [Test]
        public void Should_set_a_single_topic_as_destination_for_events()
        {
            var container = new TransportPartsContainer();

            var topology = SetupForwardingTopology(container, "sales");

            var destination = topology.DeterminePublishDestination(typeof(SomeMessageType));

            Assert.IsTrue(destination.Entities.Single().Type == EntityType.Topic);
            Assert.IsTrue(destination.Entities.Single().Path.StartsWith("bundle"));
        }

        ITopologySectionManagerInternal SetupForwardingTopology(TransportPartsContainer container, string enpointname)
        {
            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", enpointname);
            extensions.NamespacePartitioning().AddNamespace("namespace1", AzureServiceBusConnectionString.Value);

            var topology = new ForwardingTopologyInternal(container);

            topology.Initialize(settings);

            return container.Resolve<ITopologySectionManagerInternal>();
        }

        class SomeMessageType
        {
        }

        [Test]
        public void Returns_active_and_passive_namespaces_for_partitioned_sends()
        {
            var container = new TransportPartsContainer();

            // setup using FailOverNamespacePartitioning to ensure we have active and passive namespaces for a failover
            var topology = SetupForwardingTopologyWithFailoverNamespace(container, "sales");

            var destination = topology.DetermineSendDestination("operations");

            Assert.AreEqual(2, destination.Entities.Count(), "active and passive namespace should be returned");
        }

        ITopologySectionManagerInternal SetupForwardingTopologyWithFailoverNamespace(TransportPartsContainer container, string enpointname)
        {
            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", enpointname);
            extensions.NamespacePartitioning().AddNamespace("namespace1", AzureServiceBusConnectionString.Value);
            extensions.NamespacePartitioning().AddNamespace("namespace2", AzureServiceBusConnectionString.Fallback);
            extensions.NamespacePartitioning().UseStrategy<FailOverNamespacePartitioning>();

            var topology = new ForwardingTopologyInternal(container);

            topology.Initialize(settings);

            return container.Resolve<ITopologySectionManagerInternal>();
        }
    }
}