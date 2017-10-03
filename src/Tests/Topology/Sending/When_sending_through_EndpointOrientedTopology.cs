//namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.Sending
//{
//    using System.Linq;
//    using AzureServiceBus;
//    using TestUtils;
//    using Transport.AzureServiceBus;
//    using NUnit.Framework;

//    [TestFixture]
//    [Category("AzureServiceBus")]
//    public class When_sending_through_EndpointOrientedTopology
//    {
//        [Test]
//        public void Determines_that_sends_go_to_a_single_queue()
//        {
//            var topology = SetupEndpointOrientedTopology("sales");

//            var destination = topology.DetermineSendDestination("operations");

//            Assert.IsTrue(destination.Entities.Single().Type == EntityType.Queue);
//            Assert.IsTrue(destination.Entities.Single().Path == "operations");
//        }

//        [Test]
//        public void Determines_that_sends_go_to_a_single_topic_owned_by_the_endpoint()
//        {
//            var topology = SetupEndpointOrientedTopology("sales");

//            var destination = topology.DeterminePublishDestination(typeof(SomeMessageType), "sales");

//            Assert.IsTrue(destination.Entities.Single().Type == EntityType.Topic);
//            Assert.IsTrue(destination.Entities.Single().Path == "sales.events");
//        }

//        static ITopologySectionManagerInternal SetupEndpointOrientedTopology(string endpointName)
//        {
//            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
//            settings.Set<Conventions>(new Conventions());
//            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
//            settings.SetDefault("NServiceBus.Routing.EndpointName", endpointName);
//            extensions.NamespacePartitioning().AddNamespace("namespace1", AzureServiceBusConnectionString.Value);

//            var topology = new EndpointOrientedTopologyInternal();

//            topology.Initialize(settings);

//            return topology.TopologySectionManager;
//        }

//        class SomeMessageType
//        {
//        }

//        [Test]
//        public void Returns_active_and_passive_namespaces_for_partitioned_sends()
//        {
//            // setup using FailOverNamespacePartitioning to ensure we have active and passive namespaces for a failover
//            var topology = SetupEndpointOrientedTopologyWithFailoverNamespace("sales");

//            var destination = topology.DetermineSendDestination("operations");

//            Assert.AreEqual(2, destination.Entities.Count(), "active and passive namespace should be returned");
//        }

//        static ITopologySectionManagerInternal SetupEndpointOrientedTopologyWithFailoverNamespace(string endpointName)
//        {
//            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
//            settings.Set<Conventions>(new Conventions());
//            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
//            settings.SetDefault("NServiceBus.Routing.EndpointName", endpointName);
//            extensions.NamespacePartitioning().AddNamespace("namespace1", AzureServiceBusConnectionString.Value);
//            extensions.NamespacePartitioning().AddNamespace("namespace2", AzureServiceBusConnectionString.Fallback);
//            extensions.NamespacePartitioning().UseStrategy<FailOverNamespacePartitioning>();

//            var topology = new EndpointOrientedTopologyInternal();

//            topology.Initialize(settings);

//            return topology.TopologySectionManager;
//        }
//    }
//}