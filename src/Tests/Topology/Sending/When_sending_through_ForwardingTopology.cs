namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.Sending
{
    using System.Linq;
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

#pragma warning disable 618
        ITopologySectionManager SetupForwardingTopology(TransportPartsContainer container, string enpointname)
        {
            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", enpointname);
            extensions.NamespacePartitioning().AddNamespace("namespace1", AzureServiceBusConnectionString.Value);

            var topology = new ForwardingTopology(container);

            topology.Initialize(settings);

            return container.Resolve<ITopologySectionManager>();
        }
#pragma warning restore 618

        class SomeMessageType
        {
        }
    }
}