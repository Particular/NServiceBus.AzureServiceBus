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
            var topology = SetupForwardingTopology("sales");

            var destination = topology.DetermineSendDestination("operations");

            Assert.IsTrue(destination.Entities.Single().Type == EntityType.Queue);
            Assert.IsTrue(destination.Entities.Single().Path == "operations");
        }

        [Test]
        public void Should_set_a_single_topic_as_destination_for_events()
        {
            var topology = SetupForwardingTopology("sales");

            var destination = topology.DeterminePublishDestination(typeof(SomeMessageType));

            Assert.IsTrue(destination.Entities.Single().Type == EntityType.Topic);
            Assert.IsTrue(destination.Entities.Single().Path.StartsWith("bundle"));
        }

        static ITopologySectionManagerInternal SetupForwardingTopology(string enpointname)
        {
            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", enpointname);
            extensions.NamespacePartitioning().AddNamespace("namespace1", AzureServiceBusConnectionString.Value);

            var topology = new ForwardingTopologyInternal();

            topology.Initialize(settings);

            return topology.TopologySectionManager;
        }

        class SomeMessageType
        {
        }

        [Test]
        public void Returns_active_and_passive_namespaces_for_partitioned_sends()
        {
            // setup using FailOverNamespacePartitioning to ensure we have active and passive namespaces for a failover
            var topology = SetupForwardingTopologyWithFailoverNamespace("sales");

            var destination = topology.DetermineSendDestination("operations");

            Assert.AreEqual(2, destination.Entities.Count(), "active and passive namespace should be returned");
        }

        static ITopologySectionManagerInternal SetupForwardingTopologyWithFailoverNamespace(string enpointname)
        {
            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", enpointname);
            extensions.NamespacePartitioning().AddNamespace("namespace1", AzureServiceBusConnectionString.Value);
            extensions.NamespacePartitioning().AddNamespace("namespace2", AzureServiceBusConnectionString.Fallback);
            extensions.NamespacePartitioning().UseStrategy(new FailOverNamespacePartitioning());

            var topology = new ForwardingTopologyInternal();

            topology.Initialize(settings);

            return topology.TopologySectionManager;
        }
    }
}