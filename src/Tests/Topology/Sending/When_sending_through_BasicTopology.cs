namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Linq;
    using Azure.WindowsAzureServiceBus.Tests;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sending_through_BasicTopology
    {
        [Test]
        public void Determines_that_sends_go_to_a_single_queue()
        {
            // setting up the environment
            var container = new FuncBuilder();

            var topology = SetupBasicTopology(container, "sales");

            var destination = topology.DetermineSendDestination("operations");

            Assert.IsTrue(destination.Entities.Single().Type == EntityType.Queue);
            Assert.IsTrue(destination.Entities.Single().Path == "operations");
        }

        [Test]
        public void Determines_that_direct_publishing_is_not_supported()
        {
            var container = new FuncBuilder();

            var topology = SetupBasicTopology(container, "sales");

            Assert.Throws<NotSupportedException>(() => topology.DeterminePublishDestination(typeof(object)));
        }
        
        BasicTopology SetupBasicTopology(FuncBuilder container, string enpointname)
        {
            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault<EndpointName>(new EndpointName(enpointname));
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(AzureServiceBusConnectionString.Value);

            var topology = new BasicTopology(settings, container);

            topology.InitializeSettings();
            topology.InitializeContainer();

            return topology;
        }
    }
}
