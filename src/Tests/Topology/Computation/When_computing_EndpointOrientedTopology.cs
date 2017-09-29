namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.Computation
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;
    using Transport;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_computing_EndpointOrientedTopology
    {
        static string Connectionstring = "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        static string Name = "name";

        [Test]
        public async Task Determines_the_namespace_from_partitioning_strategy()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            settings.Set<Conventions>(new Conventions());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");

            extensions.NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var definition = await DetermineResourcesToCreate(settings);

            // ReSharper disable once RedundantArgumentDefaultValue
            var namespaceInfo = new RuntimeNamespaceInfo(Name, Connectionstring);
            Assert.IsTrue(definition.Namespaces.Any(nsi => nsi == namespaceInfo));
        }

        [Test]
        public async Task Determines_there_should_be_a_queue_with_same_name_as_endpointname()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            settings.Set<Conventions>(new Conventions());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");
            extensions.NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var definition = await DetermineResourcesToCreate(settings);

            Assert.AreEqual(1, definition.Entities.Count(ei => ei.Path == "sales" && ei.Type == EntityType.Queue && ei.Namespace.ConnectionString == Connectionstring));
        }

        [TestCase("Path is too long", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        [TestCase("Path contains invalid character", "input%queue")]
        public async Task Should_fail_sanitization_for_invalid_endpoint_name(string reasonToFail, string endpointName)
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            settings.Set<Conventions>(new Conventions());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault("NServiceBus.Routing.EndpointName", endpointName);
            extensions.NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var topology = new EndpointOrientedTransportInfrastructure(settings);
            await topology.Start();

            var sectionManager = topology.topologyManager;
            Assert.Throws<Exception>(() => sectionManager.DetermineResourcesToCreate(new QueueBindings(), endpointName), "Was expected to fail: " + reasonToFail);
        }

        [Test]
        public async Task Determines_there_should_be_a_topic_with_same_name_as_endpointname_followed_by_dot_events()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            settings.Set<Conventions>(new Conventions());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");
            extensions.NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var definition = await DetermineResourcesToCreate(settings);

            Assert.AreEqual(1, definition.Entities.Count(ei => ei.Path == "sales.events" && ei.Type == EntityType.Topic && ei.Namespace.ConnectionString == Connectionstring));
        }

        static async Task<TopologySectionInternal> DetermineResourcesToCreate(SettingsHolder settings)
        {
            var topology = new EndpointOrientedTransportInfrastructure(settings);
            await topology.Start();

            var sectionManager = topology.topologyManager;

            var definition = sectionManager.DetermineResourcesToCreate(new QueueBindings(), "sales");
            return definition;
        }
    }
}
