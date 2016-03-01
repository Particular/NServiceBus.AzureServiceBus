namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Linq;
    using Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.Routing;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sending_through_StandardTopology
    {
        [Test]
        public void Determines_that_sends_go_to_a_single_queue()
        {
            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = SetupStandardTopology(container, "sales");

            var destination = topology.DetermineSendDestination("operations");

            Assert.IsTrue(destination.Entities.Single().Type == EntityType.Queue);
            Assert.IsTrue(destination.Entities.Single().Path == "operations");
        }

        [Test]
        public void Determines_that_sends_go_to_a_single_topic_owned_by_the_endpoint()
        {
            var container = new TransportPartsContainer();

            var topology = SetupStandardTopology(container, "sales");

            var destination = topology.DeterminePublishDestination(typeof(SomeMessageType));

            Assert.IsTrue(destination.Entities.Single().Type == EntityType.Topic);
            Assert.IsTrue(destination.Entities.Single().Path == "sales.events");
        }

        ITopologySectionManager SetupStandardTopology(TransportPartsContainer container, string enpointname)
        {
            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault<EndpointName>(new EndpointName(enpointname));
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("namespace1", AzureServiceBusConnectionString.Value);

            var topology = new StandardTopology(container);

            topology.Initialize(settings);

            return container.Resolve<ITopologySectionManager>();
        }

        class SomeMessageType
        {
        }
    }
}