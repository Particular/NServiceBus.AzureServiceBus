namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.Computation
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using NUnit.Framework;
    using Settings;
    using Transport;
    using Transport.AzureServiceBus;

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
            settings.Set(new Conventions());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");
            settings.SetDefault("NServiceBus.SharedQueue", "sales");

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
            settings.Set(new Conventions());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");
            settings.SetDefault("NServiceBus.SharedQueue", "sales");

            extensions.NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var definition = await DetermineResourcesToCreate(settings);

            Assert.AreEqual(1, definition.Entities.Count(ei => ei.Path == "sales" && ei.Type == EntityType.Queue && ei.Namespace.ConnectionString == Connectionstring));
        }

        [TestCase("Path is too long", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        [TestCase("Path contains invalid character", "input%queue")]
        public async Task Should_fail_sanitization_for_invalid_endpoint_name(string reasonToFail, string endpointName)
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            settings.Set(new Conventions());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");
            settings.SetDefault("NServiceBus.SharedQueue", "sales");

            extensions.NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var topology = new EndpointOrientedTransportInfrastructure(settings);
            await topology.Start();

            var sectionManager = topology.topologyManager;
            Assert.Throws<Exception>(() => sectionManager.DetermineQueuesToCreate(new QueueBindings(), endpointName), "Was expected to fail: " + reasonToFail);
        }

        [Test]
        public async Task Determines_there_should_be_a_topic_with_same_name_as_endpointname_followed_by_dot_events()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            settings.Set(new Conventions());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");
            settings.SetDefault("NServiceBus.SharedQueue", "sales");

            extensions.NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var definition = await DetermineResourcesToCreate(settings);

            Assert.AreEqual(1, definition.Entities.Count(ei => ei.Path == "sales.events" && ei.Type == EntityType.Topic && ei.Namespace.ConnectionString == Connectionstring));
        }

        static async Task<TopologySectionInternal> DetermineResourcesToCreate(SettingsHolder settings)
        {
            var topology = new EndpointOrientedTransportInfrastructure(settings);
            await topology.Start();

            var sectionManager = topology.topologyManager;

            var queueDefinition = sectionManager.DetermineQueuesToCreate(new QueueBindings(), "sales");
            var topicDefinition = sectionManager.DetermineTopicsToCreate("sales");
            return new TopologySectionInternal
            {
                Namespaces = queueDefinition.Namespaces.Union(topicDefinition.Namespaces),
                Entities = queueDefinition.Entities.Union(topicDefinition.Entities)
            };
        }
    }
}
