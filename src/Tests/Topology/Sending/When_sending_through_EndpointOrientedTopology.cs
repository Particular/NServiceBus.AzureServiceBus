namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.Sending
{
    using System.Linq;
    using TestUtils;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sending_through_EndpointOrientedTopology
    {
        [Test]
        public void Determines_that_sends_go_to_a_single_queue()
        {
            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = SetupEndpointOrientedTopology(container, "sales");

            var destination = topology.DetermineSendDestination("operations");

            Assert.IsTrue(destination.Entities.Single().Type == EntityType.Queue);
            Assert.IsTrue(destination.Entities.Single().Path == "operations");
        }

        [Test]
        public void Determines_that_sends_go_to_a_single_topic_owned_by_the_endpoint()
        {
            var container = new TransportPartsContainer();

            var topology = SetupEndpointOrientedTopology(container, "sales");

            var destination = topology.DeterminePublishDestination(typeof(SomeMessageType));

            Assert.IsTrue(destination.Entities.Single().Type == EntityType.Topic);
            Assert.IsTrue(destination.Entities.Single().Path == "sales.events");
        }

#pragma warning disable 618
        ITopologySectionManagerInternal SetupEndpointOrientedTopology(TransportPartsContainer container, string enpointname)
        {
            var settings = new SettingsHolder();
            settings.Set<Conventions>(new Conventions());
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", enpointname);
            extensions.NamespacePartitioning().AddNamespace("namespace1", AzureServiceBusConnectionString.Value);

            var topology = new EndpointOrientedTopology(container);

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
            var topology = SetupEndpointOrientedTopologyWithFailoverNamespace(container, "sales");

            var destination = topology.DetermineSendDestination("operations");

            Assert.AreEqual(2, destination.Entities.Count(), "active and passive namespace should be returned");
        }

#pragma warning disable 618
        ITopologySectionManagerInternal SetupEndpointOrientedTopologyWithFailoverNamespace(TransportPartsContainer container, string enpointname)
        {
            var settings = new SettingsHolder();
            settings.Set<Conventions>(new Conventions());
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", enpointname);
            extensions.NamespacePartitioning().AddNamespace("namespace1", AzureServiceBusConnectionString.Value);
            extensions.NamespacePartitioning().AddNamespace("namespace2", AzureServiceBusConnectionString.Fallback);
            extensions.NamespacePartitioning().UseStrategy<FailOverNamespacePartitioning>();

            var topology = new EndpointOrientedTopology(container);

            topology.Initialize(settings);

            return container.Resolve<ITopologySectionManagerInternal>();
        }
#pragma warning restore 618
    }
}