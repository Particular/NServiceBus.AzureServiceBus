namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.Computation
{
    using System;
    using System.Linq;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;
    using Transport;

#pragma warning disable 618
    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_computing_EndpointOrientedTopology
    {
        static string Connectionstring = "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        static string Name = "name";

        [Test]
        public void Determines_the_namespace_from_partitioning_strategy()
        {
            var container = new TransportPartsContainer();

            var settings = new SettingsHolder();
            settings.Set<Conventions>(new Conventions());
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");

            extensions.NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var definition = DetermineResourcesToCreate(settings, container);

            // ReSharper disable once RedundantArgumentDefaultValue
            var namespaceInfo = new RuntimeNamespaceInfo(Name, Connectionstring);
            Assert.IsTrue(definition.Namespaces.Any(nsi => nsi == namespaceInfo));
        }

        [Test]
        public void Determines_there_should_be_a_queue_with_same_name_as_endpointname()
        {
            var container = new TransportPartsContainer();

            var settings = new SettingsHolder();
            settings.Set<Conventions>(new Conventions());
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");
            extensions.NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var definition = DetermineResourcesToCreate(settings, container);

            Assert.AreEqual(1, definition.Entities.Count(ei => ei.Path == "sales" && ei.Type == EntityType.Queue && ei.Namespace.ConnectionString == Connectionstring));
        }

        [TestCase("Path is too long", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        [TestCase("Path contains invalid character", "input%queue")]
        public void Should_fail_sanitization_for_invalid_endpoint_name(string reasonToFail, string endpointName)
        {
            var container = new TransportPartsContainer();

            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault("NServiceBus.Routing.EndpointName", endpointName);
            extensions.NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var topology = new ForwardingTopologyInternal(container);

            topology.Initialize(settings);

            var sectionManager = container.Resolve<ITopologySectionManagerInternal>();
            Assert.Throws<Exception>(() => sectionManager.DetermineResourcesToCreate(new QueueBindings()), "Was expected to fail: " + reasonToFail);
        }


        [Test]
        public void Determines_there_should_be_a_topic_with_same_name_as_endpointname_followed_by_dot_events()
        {
            var container = new TransportPartsContainer();

            var settings = new SettingsHolder();
            settings.Set<Conventions>(new Conventions());
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");
            extensions.NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var definition = DetermineResourcesToCreate(settings, container);

            Assert.AreEqual(1, definition.Entities.Count(ei => ei.Path == "sales.events" && ei.Type == EntityType.Topic && ei.Namespace.ConnectionString == Connectionstring));
        }

        static TopologySectionInternal DetermineResourcesToCreate(SettingsHolder settings, TransportPartsContainer container)
        {
            var topology = new EndpointOrientedTopologyInternal(container);

            topology.Initialize(settings);

            var sectionManager = container.Resolve<ITopologySectionManagerInternal>();

            var definition = sectionManager.DetermineResourcesToCreate(new QueueBindings());
            return definition;
        }
    }
}
