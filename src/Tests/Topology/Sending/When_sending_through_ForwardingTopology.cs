namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Linq;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sending_through_ForwardingTopology
    {
        [Test]
        public void Determines_that_sends_go_to_a_single_queue()
        {
            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = SetupForwardingTopology(container, "sales");

            var destination = topology.DetermineSendDestination("operations");

            Assert.IsTrue(destination.Entities.Single().Type == EntityType.Queue);
            Assert.IsTrue(destination.Entities.Single().Path == "operations");
        }

        [Test]
        public void Determines_that_sends_can_go_to_any_topic_that_belongs_to_a_bundle()
        {
            var container = new TransportPartsContainer();

            var topology = SetupForwardingTopology(container, "sales");

            var destination = topology.DeterminePublishDestination(typeof(SomeMessageType));

            Assert.IsTrue(destination.Entities.Count() > 1);
            Assert.IsTrue(destination.Entities.First().Type == EntityType.Topic);
            Assert.IsTrue(destination.Entities.First().Path.StartsWith("bundle"));
        }

        ForwardingTopology SetupForwardingTopology(TransportPartsContainer container, string enpointname)
        {
            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault<EndpointName>(new EndpointName(enpointname));
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(AzureServiceBusConnectionString.Value);

            var topology = new ForwardingTopology();

            topology.InitializeSettings(settings);
            topology.InitializeContainer(null, container);

            return topology;
        }

        class SomeMessageType
        {
        }
    }
}